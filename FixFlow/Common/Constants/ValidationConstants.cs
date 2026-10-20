namespace FixFlow.Web.Common.Constants;

public static class ValidationConstants
{
    public static class Customer
    {
        public const int FullNameMinLength = 2;
        public const int FullNameMaxLength = 100;
        public const int PhoneNumberMaxLength = 20;
        public const int EmailMaxLength = 150;
    }

    public static class Technician
    {
        public const int FullNameMinLength = 2;
        public const int FullNameMaxLength = 100;
        public const int SpecializationMaxLength = 100;
    }

    public static class RepairRequest
    {
        public const int DeviceNameMinLength = 2;
        public const int DeviceNameMaxLength = 150;

        public const int ProblemDescriptionMinLength = 10;
        public const int ProblemDescriptionMaxLength = 1000;

        public const int DiagnosisMaxLength = 1000;
    }

    public static class SparePart
    {
        public const int NameMaxLength = 150;
        public const int SkuMaxLength = 50;
    }

    public static class RepairStatusHistory
    {
        public const int NoteMaxLength = 500;
    }
}