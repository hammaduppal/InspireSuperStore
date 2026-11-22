using System.Collections.Concurrent;

namespace MarketBal.Repository.InvoiceRP
{
    public static class ActiveTillTracker
    {
        // Key: "Till-1", Value: UserId or UserName
        private static readonly ConcurrentDictionary<string, string> _bookedTills = new();

        public static Dictionary<string, string> GetBookedTills()
        {
            return _bookedTills.ToDictionary(k => k.Key, v => v.Value);
        }

        public static bool TryBookTill(string tillName, string userId)
        {
            // If this user already holds a different till, release it first
            ReleaseByUser(userId);

            // Allow re-booking if the same user already owns it
            if (_bookedTills.TryGetValue(tillName, out var existingUser) && existingUser == userId)
                return true;

            // Atomically try to book the till
            return _bookedTills.TryAdd(tillName, userId);
        }

        public static void ReleaseByUser(string userId)
        {
            var userTills = _bookedTills.Where(x => x.Value == userId).Select(x => x.Key).ToList();
            foreach (var till in userTills)
            {
                _bookedTills.TryRemove(till, out _);
            }
        }

        public static void ReleaseTill(string tillName)
        {
            _bookedTills.TryRemove(tillName, out _);
        }
    }
}
