using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Microsoft.EntityFrameworkCore;
using MarketBal.Repository.RealEstateRP.Models;

namespace MarketBal.Repository.RealEstateRP
{
    public class RealEstateRepository
    {
        private readonly OneDb _onedb;

        public RealEstateRepository(OneDb oneDb)
        {
            _onedb = oneDb;

        }

        public List<RecompanyVM> GetCompanies()
        {
            return _onedb.Recompanies.Select(x => new RecompanyVM
            {
                RecompanyId = x.RecompanyId,
                RecontactName = x.RecontactName,
            }).ToList();
        }
        public List<RecompanyContactVM> GetContacts()
        {
            var query = _onedb.RecompanyContacts.AsQueryable();


            return query.Select(x => new RecompanyContactVM
            {
                RecompanyContactId = x.RecompanyContactId,
                FullName = x.FullName,
                Cnic = x.Cnic,
                RecontactTypeId = x.RecontactTypeId,
                RecompanyId = x.RecompanyId,
                CreatedOn = x.CreatedOn,
                CreatedBy = x.CreatedBy,
                ModifiedOn = x.ModifiedOn,
                ModifiedBy = x.ModifiedBy,
                Email = x.Email,
                MobileHome = x.MobileHome,
                MobileWork = x.MobileWork,
                LandLine = x.LandLine,
                Recompany = x.Recompany == null ? null : new RecompanyVM
                {
                    RecompanyId = x.Recompany.RecompanyId,
                    RecontactName = x.Recompany.RecontactName
                },
                RecontactType = x.RecontactType == null ? null : new RecontactTypeVM
                {
                    RecontactTypeId = x.RecontactType.RecontactTypeId,
                    RecontactTypeName = x.RecontactType.RecontactTypeName
                },
                Readdresses = x.Readdresses.Select(a => new ReaddressVM
                {
                    ReaddressId = a.ReaddressId,
                    ReaddressName = a.ReaddressName,
                    CityId = a.CityId,
                    ReaddressType = a.ReaddressType,
                    CreatedOn = a.CreatedOn,
                    CreatedBy = a.CreatedBy,
                    ModifiedOn = a.ModifiedOn,
                    ModifiedBy = a.ModifiedBy,
                    RecompanyContactId = a.RecompanyContactId,
                    City = a.City == null ? null : new CityVM { CityId = a.City.CityId, CityName = a.City.CityName }
                }).ToList()
            }).ToList();
        }

        public List<RecompanyContactVM> GetContacts(string contactType)
        {
            var query = _onedb.RecompanyContacts.AsQueryable();
            if (!string.IsNullOrEmpty(contactType))
            {
                query = query.Where(x => x.RecontactType != null && x.RecontactType.RecontactTypeName == contactType);
            }

            return query.Select(x => new RecompanyContactVM
            {
                RecompanyContactId = x.RecompanyContactId,
                FullName = x.FullName,
                Cnic = x.Cnic,
                RecontactTypeId = x.RecontactTypeId,
                RecompanyId = x.RecompanyId,
                CreatedOn = x.CreatedOn,
                CreatedBy = x.CreatedBy,
                ModifiedOn = x.ModifiedOn,
                ModifiedBy = x.ModifiedBy,
                Email = x.Email,
                MobileHome = x.MobileHome,
                MobileWork = x.MobileWork,
                LandLine = x.LandLine,
                Recompany = x.Recompany == null ? null : new RecompanyVM
                {
                    RecompanyId = x.Recompany.RecompanyId,
                    RecontactName = x.Recompany.RecontactName
                },
                RecontactType = x.RecontactType == null ? null : new RecontactTypeVM
                {
                    RecontactTypeId = x.RecontactType.RecontactTypeId,
                    RecontactTypeName = x.RecontactType.RecontactTypeName
                },
                Readdresses = x.Readdresses.Select(a => new ReaddressVM
                {
                    ReaddressId = a.ReaddressId,
                    ReaddressName = a.ReaddressName,
                    CityId = a.CityId,
                    ReaddressType = a.ReaddressType,
                    CreatedOn = a.CreatedOn,
                    CreatedBy = a.CreatedBy,
                    ModifiedOn = a.ModifiedOn,
                    ModifiedBy = a.ModifiedBy,
                    RecompanyContactId = a.RecompanyContactId,
                    City = a.City == null ? null : new CityVM { CityId = a.City.CityId, CityName = a.City.CityName }
                }).ToList()
            }).ToList();
        }

        public async Task<RecompanyContactVM> GetContact(int id)
        {
            if (id <= 0)
                return null;

            var query = _onedb.RecompanyContacts.Where(x => x.RecompanyContactId == id);

            return query.Select(x => new RecompanyContactVM
            {
                RecompanyContactId = x.RecompanyContactId,
                FullName = x.FullName,
                Cnic = x.Cnic,
                RecontactTypeId = x.RecontactTypeId,
                RecompanyId = x.RecompanyId,
                CreatedOn = x.CreatedOn,
                CreatedBy = x.CreatedBy,
                ModifiedOn = x.ModifiedOn,
                ModifiedBy = x.ModifiedBy,
                Email = x.Email,
                MobileHome = x.MobileHome,
                MobileWork = x.MobileWork,
                LandLine = x.LandLine,
                Recompany = x.Recompany == null ? null : new RecompanyVM
                {
                    RecompanyId = x.Recompany.RecompanyId,
                    RecontactName = x.Recompany.RecontactName
                },
                RecontactType = x.RecontactType == null ? null : new RecontactTypeVM
                {
                    RecontactTypeId = x.RecontactType.RecontactTypeId,
                    RecontactTypeName = x.RecontactType.RecontactTypeName
                },
                Readdresses = x.Readdresses.Select(a => new ReaddressVM
                {
                    ReaddressId = a.ReaddressId,
                    ReaddressName = a.ReaddressName,
                    CityId = a.CityId,
                    ReaddressType = a.ReaddressType,
                    CreatedOn = a.CreatedOn,
                    CreatedBy = a.CreatedBy,
                    ModifiedOn = a.ModifiedOn,
                    ModifiedBy = a.ModifiedBy,
                    RecompanyContactId = a.RecompanyContactId,
                    City = a.City == null ? null : new CityVM { CityId = a.City.CityId, CityName = a.City.CityName }
                }).ToList()
            }).FirstOrDefault();
        }

        public async Task<bool> AddContact(RecompanyContactVM modal)
        {
            // basic validation: modal must be provided and required fields must be present
            if (modal == null)
                return false;

            if (string.IsNullOrWhiteSpace(modal.FullName) || string.IsNullOrWhiteSpace(modal.MobileHome))
                return false;

            try
            {
                DateTime date = DateTime.UtcNow;
                var getcontactType = await _onedb.RecontactTypes.FirstOrDefaultAsync(x => x.RecontactTypeName == modal.RecontactTypeName);

                // Update existing contact
                if (modal.RecompanyContactId > 0)
                {
                    var existing = await _onedb.RecompanyContacts
                        .Include(r => r.Readdresses)
                        .FirstOrDefaultAsync(x => x.RecompanyContactId == modal.RecompanyContactId);

                    if (existing == null)
                        return false;

                    // update scalar fields
                    existing.Cnic = modal.Cnic;
                    existing.Email = modal.Email;
                    existing.FullName = modal.FullName;
                    existing.MobileHome = modal.MobileHome;
                    existing.MobileWork = modal.MobileWork;
                    existing.LandLine = modal.LandLine;
                    existing.RecontactTypeId = getcontactType?.RecontactTypeId ?? modal.RecontactTypeId;
                    existing.RecompanyId = modal.RecompanyId;
                    existing.ModifiedOn = date;
                    existing.ModifiedBy = modal.ModifiedBy;

                    // Update addresses if provided
                    if (modal.Readdresses != null && modal.Readdresses.Any())
                    {
                        foreach (var addrVm in modal.Readdresses)
                        {
                            if (addrVm.ReaddressId > 0)
                            {
                                var existAddr = existing.Readdresses.FirstOrDefault(a => a.ReaddressId == addrVm.ReaddressId);
                                if (existAddr != null)
                                {
                                    existAddr.ReaddressName = addrVm.ReaddressName;
                                    // only change CityId if provided (non-zero)
                                    if (addrVm.CityId != 0)
                                        existAddr.CityId = addrVm.CityId;
                                    existAddr.ReaddressType = addrVm.ReaddressType;
                                    existAddr.ModifiedOn = date;
                                    existAddr.ModifiedBy = modal.ModifiedBy;
                                }
                            }
                            else
                            {
                                existing.Readdresses.Add(new Readdress
                                {
                                    ReaddressName = addrVm.ReaddressName,
                                    CityId = addrVm.CityId == 0 ? null : addrVm.CityId,
                                    ReaddressType = addrVm.ReaddressType,
                                    CreatedOn = date,
                                    CreatedBy = modal.ModifiedBy
                                });
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(modal.Address) || modal.CityId != 0)
                    {
                        // single address fields provided: update first address or add new
                        var first = existing.Readdresses.FirstOrDefault();
                        if (first != null)
                        {
                            if (!string.IsNullOrEmpty(modal.Address))
                                first.ReaddressName = modal.Address;
                            if (modal.CityId != 0)
                                first.CityId = modal.CityId;
                            first.ModifiedOn = date;
                            first.ModifiedBy = modal.ModifiedBy;
                        }
                        else
                        {
                            existing.Readdresses.Add(new Readdress
                            {
                                ReaddressName = modal.Address,
                                CityId = modal.CityId == 0 ? null : modal.CityId,
                                ReaddressType = "Primary",
                                CreatedOn = date,
                                CreatedBy = modal.ModifiedBy
                            });
                        }
                    }

                    await _onedb.SaveChangesAsync();
                    return true;
                }
                else
                {
                    // create new contact
                    RecompanyContact con = new RecompanyContact
                    {
                        Cnic = modal.Cnic,
                        CreatedOn = date,
                        CreatedBy = modal.CreatedBy,
                        Email = modal.Email,
                        FullName = modal.FullName,
                        MobileHome = modal.MobileHome,
                        MobileWork = modal.MobileWork,
                        LandLine = modal.LandLine,
                        // handle null contact type: prefer found type id, fall back to provided id (may be null)
                        RecontactTypeId = getcontactType?.RecontactTypeId ?? modal.RecontactTypeId,
                        RecompanyId = modal.RecompanyId
                    };

                    // Add addresses if provided in the view model
                    var addresses = new List<Readdress>();

                    if (modal.Readdresses != null && modal.Readdresses.Any())
                    {
                        addresses.AddRange(modal.Readdresses.Select(a => new Readdress
                        {
                            ReaddressName = a.ReaddressName,
                            CityId = a.CityId == 0 ? null : a.CityId,
                            ReaddressType = a.ReaddressType,
                            CreatedOn = date,
                            CreatedBy = modal.CreatedBy
                        }));
                    }
                    else if (!string.IsNullOrEmpty(modal.Address))
                    {
                        addresses.Add(new Readdress
                        {
                            ReaddressName = modal.Address,
                            CityId = modal.CityId == 0 ? null : modal.CityId,
                            ReaddressType = "Primary",
                            CreatedOn = date,
                            CreatedBy = modal.CreatedBy
                        });
                    }

                    if (addresses.Any())
                    {
                        con.Readdresses = addresses;
                    }

                    _onedb.RecompanyContacts.Add(con);
                    await _onedb.SaveChangesAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<RepropertyTypeVM>> GetPropertyTypes()
        {
            return await _onedb.RepropertyTypes.Select(a => new RepropertyTypeVM
            {
                PropertyTypeId = a.PropertyTypeId,
                PropertyTypeName = a.PropertyTypeName,
            }).ToListAsync();
        }
        public async Task<List<PropertyPurposeTypeVM>> GetPropertyPurposeTypes()
        {
            return await _onedb.PropertyPurposeTypes.Select(a => new PropertyPurposeTypeVM
            {
                PurposeTypeId = a.PurposeTypeId,
                PurposeTypeName = a.PurposeTypeName,
            }).ToListAsync();
        }




        public async Task<bool> AddProperty(AddPropertyModel addProperty, List<APIImageContentResponse> uploadResult)
        {
            if (addProperty == null)
                return false;

            try
            {
                DateTime now = DateTime.Now;

                var property = new Reproperty
                {
                    Title = addProperty.Title,
                    Description = addProperty.Description,
                    Price = addProperty.Price.Value,
                    PropertyTypeId = addProperty.PropertyTypeId.Value,
                    PurposeTypeId = addProperty.PurposeTypeId.Value,
                    CreatedOn = now,
                    CreatedBy = AppDataUtility.SessionUser.Id,
                    AddressDetails = addProperty.AddressDetails,
                    BaseSizeInSqFt = addProperty.BaseSizeInSqFt.Value,

                    PropertyCode = addProperty.PropertyCode,
                    CityId = addProperty.CityId.Value,

                    // Default Flags
                    IsPriceNegotiable = addProperty.IsPriceNegotiable,
                    HasGas = addProperty.HasGas,
                    HasElectricity = addProperty.HasElectricity,
                    HasWaterSupply = addProperty.HasWaterSupply,
                    HasSewerage = addProperty.HasSewerage,
                    IsActive = true

                };

                List<PropertyMedium> pm = new List<MainModels.Models.PropertyMedium>();
                foreach (var item in addProperty.VideoUrls)
                {
                    pm.Add(new PropertyMedium
                    {
                        MediaTypeId = 2,
                        MediaUrl = item,
                        CreatedOn = now
                    });
                }
                foreach (var item in uploadResult)
                {
                    pm.Add(new PropertyMedium
                    {
                        MediaTypeId = 1,
                        MediaUrl = item.ImageUrl,
                        CreatedOn = now
                    });
                }
                
                property.PropertyMedia = pm;
                _onedb.Reproperties.Add(property);

                await _onedb.SaveChangesAsync();

                var mediums = new List<PropertyMedium>();
                int priority = 1;

               


                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
