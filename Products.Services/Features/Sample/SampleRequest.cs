using MadSoul.Common;
using MediatR;

namespace Products.Services.Features.Sample;

public class SampleRequest : DtoResponse , IRequest<DtoResponse<string>>
{
    public int Age { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
}