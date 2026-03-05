namespace LeadFlow.Domain.Enums;

/// <summary>Type of identity document provided for KYC.</summary>
public enum KycDocumentType
{
    AadhaarCard,
    PanCard,
    DrivingLicence,
    Passport,
    VoterId
}

/// <summary>Category of document stored against a resource.</summary>
public enum ResourceDocumentType
{
    Resume,
    KYC,
    Payslip,
    OfferLetter,
    ExperienceLetter,
    Other
}

/// <summary>How the reference was created.</summary>
public enum ReferenceType
{
    InternalUser,
    Lead,
    Vendor,
    Portal,
    Other
}

/// <summary>Work-mode preference for a resource.</summary>
public enum WorkModePreference
{
    OnSite,
    Remote,
    Hybrid
}
