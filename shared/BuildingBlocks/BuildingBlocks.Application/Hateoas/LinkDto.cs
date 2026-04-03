namespace BuildingBlocks.Application.Hateoas;

public class LinkDto
{
    public string Rel { get; set; } = default!;
    public string Href { get; set; } = default!;
    public string Method { get; set; } = default!;
}
