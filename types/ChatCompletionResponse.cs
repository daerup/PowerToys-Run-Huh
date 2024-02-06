#pragma warning disable CS8618
namespace PowerToys_Run_Huh.types;

public record ChatCompletionResponse
{
    public Choice[] Choices { get; init; }
    public long Created { get; init; }
    public string Id { get; init; }
    public string Object { get; init; }
    public string Model { get; init; }
}