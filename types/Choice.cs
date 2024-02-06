#pragma warning disable CS8618
namespace PowerToys_Run_Huh.types;

public record Choice
{
    public Message Message { get; init; }
    public int Index { get; init; }
    public string FinishReason { get; init; }
}