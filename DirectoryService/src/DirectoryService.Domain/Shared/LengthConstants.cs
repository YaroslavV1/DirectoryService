namespace DirectoryService.Domain.Shared;

public readonly struct LengthConstants
{
    public const short MIN_DEPARTMENT_NAME = 3;
    public const short MAX_DEPARTMENT_NAME = 150;

    public const short MIN_DEPARTMENT_ID = 3;
    public const short MAX_DEPARTMENT_ID = 150;

    public const short MIN_LOCATION_NAME = 1;
    public const short MAX_LOCATION_NAME = 120;

    public const short MIN_POSITION_NAME = 3;
    public const short MAX_POSITION_NAME = 100;
    public const int MAX_POSITION_DESCRIPTION = 1000;
}