// ReSharper disable All
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace PowerToys_Run_Huh.types;

public record Message
{
    public string Role { get; init; }
    public string Content { get; init; }
}
