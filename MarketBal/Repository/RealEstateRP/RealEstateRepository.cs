using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Microsoft.EntityFrameworkCore;
using MarketBal.Repository.RealEstateRP.Models;
using System.Collections.Immutable;

namespace MarketBal.Repository.RealEstateRP
{
    public class RealEstateRepository
    {
        private readonly OneDb _onedb;

        public RealEstateRepository(OneDb oneDb)
        {
            _onedb = oneDb;

        }

        #region ContactsDetails
        public List<RecompanyVM> GetCompanies()
        {
            return _onedb.Recompanies.Select(x => new RecompanyVM
            {
                RecompanyId = x.RecompanyId,
                RecontactName = x.RecontactName,
            }).ToList();
        }
        public async Task<List<RecompanyContactVM>> GetContacts()
        {
            return await _onedb.RecompanyContacts
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new RecompanyContactVM
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
                        City = a.City == null ? null : new CityVM
                        {
                            CityId = a.City.CityId,
                            CityName = a.City.CityName
                        }
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<RecompanyContactVM>> GetContacts(string contactType = null)
        {
            var query = _onedb.RecompanyContacts.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(contactType))
            {
                query = query.Where(x => x.RecontactType != null && x.RecontactType.RecontactTypeName == contactType);
            }

            return await query
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new RecompanyContactVM
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
                        City = a.City == null ? null : new CityVM
                        {
                            CityId = a.City.CityId,
                            CityName = a.City.CityName
                        }
                    }).ToList()
                })
                .ToListAsync();
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
        public async Task<List<RecontactTypeVM>> ContactTypes()
        {
            return await _onedb.RecontactTypes.Select(x => new RecontactTypeVM
            {
                RecontactTypeId = x.RecontactTypeId,
                RecontactTypeName = x.RecontactTypeName

            }).ToListAsync();
        }
        #endregion


        #region PropertiesSection

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

        public async Task<List<RepropertyVM>> GetAllProperties()
        {
            return await _onedb.Reproperties
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new RepropertyVM
                {
                    PropertyId = x.PropertyId,
                    Title = x.Title,
                    Description = x.Description,
                    PropertyCode = x.PropertyCode,
                    PropertyTypeId = x.PropertyTypeId,
                    PurposeTypeId = x.PurposeTypeId,
                    PropertyStatusTypeId = x.PropertyStatusTypeId,
                    CityId = x.CityId,
                    LocalityId = x.LocalityId,
                    SubLocalityId = x.SubLocalityId,
                    AddressDetails = x.AddressDetails,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    BaseSizeInSqFt = x.BaseSizeInSqFt,
                    DisplayUnitId = x.DisplayUnitId,
                    DimensionFront = x.DimensionFront,
                    DimensionDepth = x.DimensionDepth,
                    CoveredAreaSqFt = x.CoveredAreaSqFt,
                    Price = x.Price,
                    SecurityDeposit = x.SecurityDeposit,
                    LeaseDurationMonths = x.LeaseDurationMonths,
                    AdvanceRentMonths = x.AdvanceRentMonths,
                    IsPriceNegotiable = x.IsPriceNegotiable,
                    MaintenanceFee = x.MaintenanceFee,
                    Bedrooms = x.Bedrooms,
                    Bathrooms = x.Bathrooms,
                    FloorsCount = x.FloorsCount,
                    ParkingSpaces = x.ParkingSpaces,
                    ConstructionStatusTypeId = x.ConstructionStatusTypeId,
                    YearBuilt = x.YearBuilt,
                    KhasraNumber = x.KhasraNumber,
                    KhewatNumber = x.KhewatNumber,
                    KhatoniNumber = x.KhatoniNumber,
                    MouzaName = x.MouzaName,
                    WaterSourceTypeId = x.WaterSourceTypeId,
                    NocStatusTypeId = x.NocStatusTypeId,
                    PossessionStatusTypeId = x.PossessionStatusTypeId,
                    OwnershipTypeId = x.OwnershipTypeId,
                    HasGas = x.HasGas,
                    HasElectricity = x.HasElectricity,
                    HasWaterSupply = x.HasWaterSupply,
                    HasSewerage = x.HasSewerage,
                    IsCornerPlot = x.IsCornerPlot,
                    IsMainBoulevard = x.IsMainBoulevard,
                    IsParkFacing = x.IsParkFacing,
                    IsFeatured = x.IsFeatured,
                    IsActive = x.IsActive,
                    CreatedOn = x.CreatedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedOn = x.ModifiedOn,
                    ModifiedBy = x.ModifiedBy,

                    // Navigation Objects
                    City = x.City == null ? null : new CityVM
                    {
                        CityId = x.City.CityId,
                        CityName = x.City.CityName
                    },
                    Locality = x.Locality == null ? null : new LocalityVM
                    {
                        LocalityId = x.Locality.LocalityId,
                        LocalityName = x.Locality.LocalityName
                    },
                    SubLocality = x.SubLocality == null ? null : new SubLocalityVM
                    {
                        SubLocalityId = x.SubLocality.SubLocalityId,
                        SubLocalityName = x.SubLocality.SubLocalityName
                    },

                    // Child Collections
                    PropertyMedia = x.PropertyMedia
                        .OrderBy(m => m.DisplayOrder)
                        .Select(m => new PropertyMediumVM
                        {
                            PropertyMediaId = m.PropertyMediaId,
                            PropertyId = m.PropertyId,
                            MediaTypeId = m.MediaTypeId,
                            MediaUrl = m.MediaUrl,
                            Caption = m.Caption,
                            DisplayOrder = m.DisplayOrder,
                            IsFeatured = m.IsFeatured,
                            CreatedOn = m.CreatedOn
                        }).ToList(),

                    PropertyEnquiries = x.PropertyEnquiries.Select(e => new PropertyEnquiryVM
                    {
                        EnquiryId = e.EnquiryId,
                        PropertyId = e.PropertyId,
                        FullName = e.FullName,
                        Email = e.Email,
                        Phone = e.Phone,
                        Message = e.Message,
                        CreatedOn = e.CreatedOn
                    }).ToList(),

                    Amenities = x.Amenities.Select(a => new AmenityVM
                    {
                        AmenityId = a.AmenityId,
                        AmenityName = a.AmenityName,
                        IconClass = a.IconClass
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<RepropertyVM>> GetPropertiesByType(int propertyTypeId)
        {
            return await _onedb.Reproperties
                .AsNoTracking()
                .Where(x => x.PropertyTypeId == propertyTypeId)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new RepropertyVM
                {
                    PropertyId = x.PropertyId,
                    Title = x.Title,
                    Description = x.Description,
                    PropertyCode = x.PropertyCode,
                    PropertyTypeId = x.PropertyTypeId,
                    PurposeTypeId = x.PurposeTypeId,
                    PropertyStatusTypeId = x.PropertyStatusTypeId,
                    CityId = x.CityId,
                    LocalityId = x.LocalityId,
                    SubLocalityId = x.SubLocalityId,
                    AddressDetails = x.AddressDetails,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    BaseSizeInSqFt = x.BaseSizeInSqFt,
                    DisplayUnitId = x.DisplayUnitId,
                    DimensionFront = x.DimensionFront,
                    DimensionDepth = x.DimensionDepth,
                    CoveredAreaSqFt = x.CoveredAreaSqFt,
                    Price = x.Price,
                    SecurityDeposit = x.SecurityDeposit,
                    LeaseDurationMonths = x.LeaseDurationMonths,
                    AdvanceRentMonths = x.AdvanceRentMonths,
                    IsPriceNegotiable = x.IsPriceNegotiable,
                    MaintenanceFee = x.MaintenanceFee,
                    Bedrooms = x.Bedrooms,
                    Bathrooms = x.Bathrooms,
                    FloorsCount = x.FloorsCount,
                    ParkingSpaces = x.ParkingSpaces,
                    ConstructionStatusTypeId = x.ConstructionStatusTypeId,
                    YearBuilt = x.YearBuilt,
                    KhasraNumber = x.KhasraNumber,
                    KhewatNumber = x.KhewatNumber,
                    KhatoniNumber = x.KhatoniNumber,
                    MouzaName = x.MouzaName,
                    WaterSourceTypeId = x.WaterSourceTypeId,
                    NocStatusTypeId = x.NocStatusTypeId,
                    PossessionStatusTypeId = x.PossessionStatusTypeId,
                    OwnershipTypeId = x.OwnershipTypeId,
                    HasGas = x.HasGas,
                    HasElectricity = x.HasElectricity,
                    HasWaterSupply = x.HasWaterSupply,
                    HasSewerage = x.HasSewerage,
                    IsCornerPlot = x.IsCornerPlot,
                    IsMainBoulevard = x.IsMainBoulevard,
                    IsParkFacing = x.IsParkFacing,
                    IsFeatured = x.IsFeatured,
                    IsActive = x.IsActive,
                    CreatedOn = x.CreatedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedOn = x.ModifiedOn,
                    ModifiedBy = x.ModifiedBy,

                    // Navigation Objects
                    City = x.City == null ? null : new CityVM
                    {
                        CityId = x.City.CityId,
                        CityName = x.City.CityName
                    },
                    Locality = x.Locality == null ? null : new LocalityVM
                    {
                        LocalityId = x.Locality.LocalityId,
                        LocalityName = x.Locality.LocalityName
                    },
                    SubLocality = x.SubLocality == null ? null : new SubLocalityVM
                    {
                        SubLocalityId = x.SubLocality.SubLocalityId,
                        SubLocalityName = x.SubLocality.SubLocalityName
                    },

                    // Child Collections
                    PropertyMedia = x.PropertyMedia
                        .OrderBy(m => m.DisplayOrder)
                        .Select(m => new PropertyMediumVM
                        {
                            PropertyMediaId = m.PropertyMediaId,
                            PropertyId = m.PropertyId,
                            MediaTypeId = m.MediaTypeId,
                            MediaUrl = m.MediaUrl,
                            Caption = m.Caption,
                            DisplayOrder = m.DisplayOrder,
                            IsFeatured = m.IsFeatured,
                            CreatedOn = m.CreatedOn
                        }).ToList(),

                    PropertyEnquiries = x.PropertyEnquiries.Select(e => new PropertyEnquiryVM
                    {
                        EnquiryId = e.EnquiryId,
                        PropertyId = e.PropertyId,
                        FullName = e.FullName,
                        Email = e.Email,
                        Phone = e.Phone,
                        Message = e.Message,
                        CreatedOn = e.CreatedOn
                    }).ToList(),

                    Amenities = x.Amenities.Select(a => new AmenityVM
                    {
                        AmenityId = a.AmenityId,
                        AmenityName = a.AmenityName,
                        IconClass = a.IconClass
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<RepropertyVM?> GetPropertyByIdAsync(long propertyId)
        {
            return await _onedb.Reproperties
                .AsNoTracking()
                .Where(x => x.PropertyId == propertyId)
                .Select(x => new RepropertyVM
                {
                    PropertyId = x.PropertyId,
                    Title = x.Title,
                    Description = x.Description,
                    PropertyCode = x.PropertyCode,
                    PropertyTypeId = x.PropertyTypeId,
                    PurposeTypeId = x.PurposeTypeId,
                    PropertyStatusTypeId = x.PropertyStatusTypeId,
                    CityId = x.CityId,
                    LocalityId = x.LocalityId,
                    SubLocalityId = x.SubLocalityId,
                    AddressDetails = x.AddressDetails,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    BaseSizeInSqFt = x.BaseSizeInSqFt,
                    DisplayUnitId = x.DisplayUnitId,
                    DimensionFront = x.DimensionFront,
                    DimensionDepth = x.DimensionDepth,
                    CoveredAreaSqFt = x.CoveredAreaSqFt,
                    Price = x.Price,
                    SecurityDeposit = x.SecurityDeposit,
                    LeaseDurationMonths = x.LeaseDurationMonths,
                    AdvanceRentMonths = x.AdvanceRentMonths,
                    IsPriceNegotiable = x.IsPriceNegotiable,
                    MaintenanceFee = x.MaintenanceFee,
                    Bedrooms = x.Bedrooms,
                    Bathrooms = x.Bathrooms,
                    FloorsCount = x.FloorsCount,
                    ParkingSpaces = x.ParkingSpaces,
                    ConstructionStatusTypeId = x.ConstructionStatusTypeId,
                    YearBuilt = x.YearBuilt,
                    KhasraNumber = x.KhasraNumber,
                    KhewatNumber = x.KhewatNumber,
                    KhatoniNumber = x.KhatoniNumber,
                    MouzaName = x.MouzaName,
                    WaterSourceTypeId = x.WaterSourceTypeId,
                    NocStatusTypeId = x.NocStatusTypeId,
                    PossessionStatusTypeId = x.PossessionStatusTypeId,
                    OwnershipTypeId = x.OwnershipTypeId,
                    HasGas = x.HasGas,
                    HasElectricity = x.HasElectricity,
                    HasWaterSupply = x.HasWaterSupply,
                    HasSewerage = x.HasSewerage,
                    IsCornerPlot = x.IsCornerPlot,
                    IsMainBoulevard = x.IsMainBoulevard,
                    IsParkFacing = x.IsParkFacing,
                    IsFeatured = x.IsFeatured,
                    IsActive = x.IsActive,
                    CreatedOn = x.CreatedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedOn = x.ModifiedOn,
                    ModifiedBy = x.ModifiedBy,

                    // Navigation Objects
                    City = x.City == null ? null : new CityVM
                    {
                        CityId = x.City.CityId,
                        CityName = x.City.CityName
                    },
                    Locality = x.Locality == null ? null : new LocalityVM
                    {
                        LocalityId = x.Locality.LocalityId,
                        LocalityName = x.Locality.LocalityName
                    },
                    SubLocality = x.SubLocality == null ? null : new SubLocalityVM
                    {
                        SubLocalityId = x.SubLocality.SubLocalityId,
                        SubLocalityName = x.SubLocality.SubLocalityName
                    },

                    // Child Collections
                    PropertyMedia = x.PropertyMedia
                        .OrderBy(m => m.DisplayOrder)
                        .Select(m => new PropertyMediumVM
                        {
                            PropertyMediaId = m.PropertyMediaId,
                            PropertyId = m.PropertyId,
                            MediaTypeId = m.MediaTypeId,
                            MediaUrl = m.MediaUrl,
                            Caption = m.Caption,
                            DisplayOrder = m.DisplayOrder,
                            IsFeatured = m.IsFeatured,
                            CreatedOn = m.CreatedOn
                        }).ToList(),

                    PropertyEnquiries = x.PropertyEnquiries.Select(e => new PropertyEnquiryVM
                    {
                        EnquiryId = e.EnquiryId,
                        PropertyId = e.PropertyId,
                        FullName = e.FullName,
                        Email = e.Email,
                        Phone = e.Phone,
                        Message = e.Message,
                        CreatedOn = e.CreatedOn
                    }).ToList(),

                    Amenities = x.Amenities.Select(a => new AmenityVM
                    {
                        AmenityId = a.AmenityId,
                        AmenityName = a.AmenityName,
                        IconClass = a.IconClass
                    }).ToList()
                })
                .FirstOrDefaultAsync();
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
                foreach (var item in uploadResult)
                {
                    pm.Add(new PropertyMedium
                    {
                        MediaTypeId = (int)PropertyMediumTypes.Images,
                        MediaUrl = item.ImageUrl,
                        CreatedOn = now
                    });
                }
                foreach (var item in addProperty.VideoUrls)
                {
                    pm.Add(new PropertyMedium
                    {
                        MediaTypeId = (int)PropertyMediumTypes.Videos,
                        MediaUrl = item,
                        CreatedOn = now
                    });
                }


                property.PropertyMedia = pm;
                _onedb.Reproperties.Add(property);

                await _onedb.SaveChangesAsync();



                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region LinksData

        public async Task<List<RelinksDatumVM>> LinksData(string mediatype)
        {
            if (Enum.TryParse<RealEstateMediaTypes>(mediatype, ignoreCase: true, out var parsedMedia))
            {
                int mediaTypeId = (int)parsedMedia;

                return await _onedb.RelinksData
                    .Where(x => x.LinkDataTypeId == mediaTypeId)
                    .Select(x => new RelinksDatumVM
                    {
                        RelinksDataId = x.RelinksDataId,
                        Title = x.Title,
                        Description = x.Description,
                        RelinksFiles = x.RelinksFiles.Select(y => new RelinksFileVM
                        {
                            ReLinkFileUrl = y.ReLinkFileUrl,
                            ReLinkFileCaption = y.ReLinkFileCaption,
                            ReLinkFileSortOrder = y.ReLinkFileSortOrder
                        }).ToList()
                    })
                    .ToListAsync();
            }
            else
            {
                throw new ArgumentException($"Invalid media type: {mediatype}");
            }
        }

        public async Task<RelinksDatumVM> GetLinksDataById(int mediaTypeId)
        {


            return await _onedb.RelinksData
                .Where(x => x.RelinksDataId == mediaTypeId)
                .Select(x => new RelinksDatumVM
                {
                    RelinksDataId = x.RelinksDataId,
                    Title = x.Title,
                    Description = x.Description,
                    RelinksFiles = x.RelinksFiles.Select(y => new RelinksFileVM
                    {

                        ReLinkFileUrl = y.ReLinkFileUrl,
                        ReLinkFileCaption = y.ReLinkFileCaption,
                        ReLinkFileSortOrder = y.ReLinkFileSortOrder
                    }).ToList()
                })
                .FirstOrDefaultAsync();

        }

        public async Task<bool> AddMedia(PropertyMediumFormSubmit model, List<APIImageContentResponse> uploadResult)
        {

            DateTime now = DateTime.Now;
            RelinksDatum um = new RelinksDatum();
            um.Title = model.Title;
            um.Description = model.Description;
            um.LinkDataTypeId = model.SelectedMediaType;
            if (model.RelinksDataId > 0)
            {
                // Update existing RelinksData entry: update title/description and append any newly added files/urls
                var media = await _onedb.RelinksData
                    .Include(r => r.RelinksFiles)
                    .Where(x => x.RelinksDataId == model.RelinksDataId)
                    .FirstOrDefaultAsync();

                if (media == null)
                {
                    return false;
                }

                media.Title = model.Title;
                media.Description = model.Description;

                // Determine starting sort order
                int i = 0;
                if (media.RelinksFiles != null && media.RelinksFiles.Count > 0)
                {
                    try { i = media.RelinksFiles.Max(f => f.ReLinkFileSortOrder.Value); } catch { i = media.RelinksFiles.Count; }
                }

                // Append new video URLs (client should only send newly added URLs)
                if (model.VideoUrls != null)
                {
                    foreach (var item in model.VideoUrls)
                    {
                        if (string.IsNullOrWhiteSpace(item)) continue;
                        // avoid duplicate urls
                        if (media.RelinksFiles != null && media.RelinksFiles.Any(f => f.ReLinkFileUrl == item)) continue;

                        var rf = new RelinksFile
                        {
                            ReLinkFileUrl = item,
                            ReLinkFileCaption = model.Title,
                            ReLinkFileSortOrder = ++i,
                            MediaTypeId = (int)PropertyMediumTypes.Images
                        };

                        media.RelinksFiles.Add(rf);
                    }
                }

                // Append newly uploaded images
                if (uploadResult != null)
                {
                    foreach (var item in uploadResult)
                    {
                        if (item == null || string.IsNullOrWhiteSpace(item.ImageUrl)) continue;
                        if (media.RelinksFiles != null && media.RelinksFiles.Any(f => f.ReLinkFileUrl == item.ImageUrl)) continue;

                        var rf = new RelinksFile
                        {
                            ReLinkFileUrl = item.ImageUrl,
                            ReLinkFileCaption = model.Title,
                            ReLinkFileSortOrder = ++i,
                            MediaTypeId = (int)PropertyMediumTypes.Images
                        };

                        media.RelinksFiles.Add(rf);
                    }
                }

                await _onedb.SaveChangesAsync();
                return true;
            }
            else
            {
                List<RelinksFile> pm = new List<RelinksFile>();
                int i = 1;
                if (model.VideoUrls != null)
                {
                    foreach (var item in model.VideoUrls)
                    {
                        pm.Add(new RelinksFile
                        {
                            ReLinkFileUrl = item,
                            ReLinkFileCaption = model.Title,
                            ReLinkFileSortOrder = i++,
                            MediaTypeId = (int)PropertyMediumTypes.Images
                        });
                    }

                }
                if (uploadResult != null)
                {
                    foreach (var item in uploadResult)
                    {

                        pm.Add(new RelinksFile
                        {
                            ReLinkFileUrl = item.ImageUrl,
                            ReLinkFileCaption = model.Title,
                            ReLinkFileSortOrder = i++,
                            MediaTypeId = (int)PropertyMediumTypes.Images
                        });
                    }
                }
                um.RelinksFiles = pm;
                _onedb.RelinksData.Add(um);
                await _onedb.SaveChangesAsync();
                return true;
            }


        }


        #endregion





    }
}
