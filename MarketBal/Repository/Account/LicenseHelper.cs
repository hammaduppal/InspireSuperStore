using MainModels.DTOModels;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MarketBal.Repository.Account
{
    public static class SimpleLicenseHelper
    {
        // Keep this secret safe in configuration/environment and do not ship it to clients
        private const string DefaultSecret = "REPLACE_WITH_YOUR_STRONG_SECRET_KEY";

        public static string GenerateLicenseKey(
            string requestCode,
            string serverFingerprint,
            DateTime expiryDateUtc,
            string? secret = null)
        {
            if (string.IsNullOrWhiteSpace(requestCode))
                throw new ArgumentException("Request code is required.", nameof(requestCode));

            if (string.IsNullOrWhiteSpace(serverFingerprint))
                throw new ArgumentException("Server fingerprint is required.", nameof(serverFingerprint));

            string normalizedReq = requestCode.Trim().ToUpperInvariant();
            string fpHash = ComputeSha256Hex(serverFingerprint.Trim().ToUpperInvariant())[..16];
            string expiryPart = expiryDateUtc.ToUniversalTime().ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            string payload = $"{normalizedReq}|{fpHash}|{expiryPart}";
            string payloadBase64 = ToBase64Url(payload);
            string signature = ComputeHmacHex(payloadBase64, secret ?? DefaultSecret)[..16];

            return $"{payloadBase64}.{signature}";
        }

        public static (bool IsValid, DateTime? ExpiryUtc, string Message) ValidateLicenseKey(
            string? licenseKey,
            string? expectedRequestCode,
            string? currentServerFingerprint,
            string? secret = null)
        {
            if (string.IsNullOrWhiteSpace(licenseKey) || string.IsNullOrWhiteSpace(expectedRequestCode))
                return (false, null, "License key or request code is missing.");

            var keyParts = licenseKey.Trim().Split('.', 2);
            if (keyParts.Length != 2)
                return (false, null, "Invalid license key format.");

            string payloadBase64 = keyParts[0];
            string providedSigHex = keyParts[1].Trim().ToUpperInvariant();

            string expectedSigHex = ComputeHmacHex(payloadBase64, secret ?? DefaultSecret).ToUpperInvariant()[..16];
            byte[] providedBytes = Encoding.UTF8.GetBytes(providedSigHex);
            byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedSigHex);

            if (providedBytes.Length != expectedBytes.Length ||
                !CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes))
            {
                return (false, null, "License signature is invalid or tampered.");
            }

            string payload;
            try
            {
                payload = FromBase64Url(payloadBase64);
            }
            catch
            {
                return (false, null, "Malformed license payload.");
            }

            var segments = payload.Split('|');
            if (segments.Length != 3)
                return (false, null, "Invalid license payload structure.");

            string keyRequestCode = segments[0];
            string keyFpHash = segments[1];
            string expiryStr = segments[2];

            if (!string.Equals(keyRequestCode, expectedRequestCode.Trim(), StringComparison.OrdinalIgnoreCase))
                return (false, null, "License key does not match this request code.");

            if (!string.IsNullOrWhiteSpace(currentServerFingerprint))
            {
                string currentFpHash = ComputeSha256Hex(currentServerFingerprint.Trim().ToUpperInvariant())[..16];
                if (!string.Equals(keyFpHash, currentFpHash, StringComparison.OrdinalIgnoreCase))
                    return (false, null, "License key does not match this server fingerprint.");
            }

            if (!DateTime.TryParseExact(
                    expiryStr,
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out DateTime parsedExpiryDate))
            {
                return (false, null, "Invalid expiry date in license key.");
            }

            // Valid through the end of the expiry day in UTC
            DateTime validUntilUtc = parsedExpiryDate.Date.AddDays(1).AddTicks(-1);
            if (DateTime.UtcNow > validUntilUtc)
                return (false, validUntilUtc, $"License expired on {parsedExpiryDate:yyyy-MM-dd} UTC.");

            return (true, validUntilUtc, $"License valid until {parsedExpiryDate:yyyy-MM-dd} UTC.");
        }

        private static string ComputeHmacHex(string input, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash);
        }

        private static string ComputeSha256Hex(string input)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }

        private static string ToBase64Url(string plainText)
        {
            string b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
            return b64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string FromBase64Url(string base64Url)
        {
            string padded = base64Url.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        }
















    }

}
