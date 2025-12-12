namespace MarketBal.Repository.SystemRP
{
    public interface ILicenseService
    {
        /// <summary>
        /// Determines if license is valid for this server now.
        /// </summary>
        Task<bool> IsLicenseValid(CancellationToken ct = default);

        /// <summary>
        /// Force a re-check (e.g. after installing a license).
        /// Returns the new result.
        /// </summary>
        Task<bool> RefreshAndCheck(CancellationToken ct = default);
    }
}
