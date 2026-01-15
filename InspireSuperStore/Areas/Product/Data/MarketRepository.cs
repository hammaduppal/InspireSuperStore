using System.Threading.Tasks;
using MainModels.DTOModels;
using MainModels.Models;
using Microsoft.EntityFrameworkCore;

namespace InspireSuperStore.Areas.Product.Data
{
    public class MarketRepository
    {
        private readonly OneDb _oneDb;
        public MarketRepository(OneDb oneDb)
        {
            _oneDb = oneDb;
        }
        public async Task<List<CouponVM>> GetAllCoupons()
        {
            var result = await _oneDb.Coupons.Select(x=>new CouponVM
            {
                 CouponId=x.CouponId,
                 CouponName=x.CouponName,
                 CouponCode= x.CouponCode,
                 StartDate = x.StartDate,
                 EndDate = x.EndDate
            }).ToListAsync();
            return result;
        }

        public async Task<bool> CreateCoupon(CouponVM coupon)
        {
            try
            {
                if (coupon == null)
                    return false;

                var newCoupon = new Coupon
                {
                    CouponId = Guid.NewGuid(),
                    CouponCode = coupon.CouponCode,
                    CouponName = coupon.CouponName,
                    UsageLimitPerUser = coupon.UsageLimitPerUser,
                    MaxTotalUsage = coupon.MaxTotalUsage,
                    StartDate = coupon.StartDate,
                    EndDate = coupon.EndDate,
                    MinQuantity = coupon.MinQuantity,
                    MinCartAmount = coupon.MinCartAmount,
                    DiscountType = coupon.DiscountType,
                    DiscountValue = coupon.DiscountValue,
                    AllowStacking = coupon.AllowStacking,
                    IsActive = coupon.IsActive,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _oneDb.Coupons.Add(newCoupon);
                await _oneDb.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<CouponVM> GetCoupon(Guid couponId)
        {
            return await _oneDb.Coupons
                .Where(c => c.CouponId == couponId)
                .Select(c => new CouponVM
                {
                    CouponId = c.CouponId,
                    CouponCode = c.CouponCode,
                    CouponName = c.CouponName,
                    UsageLimitPerUser = c.UsageLimitPerUser,
                    MaxTotalUsage = c.MaxTotalUsage,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    MinQuantity = c.MinQuantity,
                    MinCartAmount = c.MinCartAmount,
                    DiscountType = c.DiscountType,
                    DiscountValue = c.DiscountValue,
                    AllowStacking = c.AllowStacking,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> EditCoupon(CouponVM model)
        {
            if (model == null || model.CouponId == Guid.Empty)
                return false;

            // Get existing coupon from DB
            var existingCoupon = await _oneDb.Coupons
                .FirstOrDefaultAsync(c => c.CouponId == model.CouponId);

            if (existingCoupon == null)
                return false;

            // Update fields
            existingCoupon.CouponCode = model.CouponCode;
            existingCoupon.CouponName = model.CouponName;
            existingCoupon.UsageLimitPerUser = model.UsageLimitPerUser;
            existingCoupon.MaxTotalUsage = model.MaxTotalUsage;
            existingCoupon.StartDate = model.StartDate;
            existingCoupon.EndDate = model.EndDate;
            existingCoupon.MinQuantity = model.MinQuantity;
            existingCoupon.MinCartAmount = model.MinCartAmount;
            existingCoupon.DiscountType = model.DiscountType;
            existingCoupon.DiscountValue = model.DiscountValue;
            existingCoupon.AllowStacking = model.AllowStacking;
            existingCoupon.IsActive = model.IsActive;
            existingCoupon.UpdatedDate = DateTime.UtcNow;
          
            try
            {
                await _oneDb.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task <bool> AssignVariants(Guid couponId, List<Guid> variantIds)
        {
            var existingVariantIds = await _oneDb.CouponProducts
                .Where(x => x.CouponId == couponId)
                .Select(x => x.ProductVariantId)
                .ToListAsync();

            var newLinks = variantIds
                .Where(id => !existingVariantIds.Contains(id))
                .Select(id => new CouponProduct
                {
                    CouponProductId = Guid.NewGuid(),
                    CouponId = couponId,
                    ProductVariantId = id
                });

            _oneDb.CouponProducts.AddRange(newLinks);
            await _oneDb.SaveChangesAsync();
            return true;
        }

        public async Task<List<Guid>> GetAssignedVariantIds(Guid couponId)
        {
            var ids = await _oneDb.CouponProducts
                .Where(x => x.CouponId == couponId)
                .Select(x => x.ProductVariantId)
                .ToListAsync();

            return ids;
        }
        public async Task<object> GetAssignedVariants(Guid couponId)
        {
            var data = await (
                from cp in _oneDb.CouponProducts
                join pv in _oneDb.ProductVariants on cp.ProductVariantId equals pv.VariantId
                join p in _oneDb.Products on pv.ProductId equals p.ProductId
                where cp.CouponId == couponId
                select new
                {
                    cp.CouponProductId,
                    pv.VariantId,
                    p.ProductName,
                    pv.BarCode,
                    CurrentPrice = pv.BranchStocks.Where(x=>x.ProductVariantId==pv.VariantId).Select(bs=>bs.RetailPrice)
                }
            ).ToListAsync();

            return data;
        }
    }
}
