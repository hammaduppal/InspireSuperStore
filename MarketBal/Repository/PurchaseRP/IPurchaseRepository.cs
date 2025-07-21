using MainModels.DTOModels;

namespace MarketBal.Repository.PurchaseRP
{
    public interface IPurchaseRepository
    {
        Task<int> SavePurchase(PurchaseDataDto model);
        



    }
}
