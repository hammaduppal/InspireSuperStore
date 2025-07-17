using MainModels.DTOModels;

namespace MarketBal.Helper
{
    public static class PurchaseNumberGenerator
    {
        public static string Generate(string purchaseTypePrefix, int? supplierId = null)
        {
            string prefix = GetShortName(purchaseTypePrefix);
            string date = DateTime.UtcNow.ToString("yyyyMMdd");
            string supplierCode = supplierId.ToString();
            var random = new Random();
            string randomSuffix = random.Next(1000, 9999).ToString();
            return $"{prefix}-{date}-{supplierCode}-{randomSuffix}";
        }
        public static string GetShortName(string typeName)
        {
            if (typeName== AppConstants.PurchaseType.Requisition.ToString())
            {
                return "REQ";
            }
            else if (typeName == AppConstants.PurchaseType.Receiving.ToString())
            {
                return "RN";

            }
            else if (typeName == AppConstants.PurchaseType.PurchaseOrder.ToString())
            {
                return "PO";

            }
            else
            {
                return "UNK";
            }
        }
    }

}
