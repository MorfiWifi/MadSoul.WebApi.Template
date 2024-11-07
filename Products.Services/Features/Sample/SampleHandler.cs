using MadSoul.Common;
using MediatR;

namespace Products.Services.Features.Sample;

public class SampleHandler : IRequestHandler<SampleRequest , DtoResponse<string>>
// public class SampleHandler(Dependency using DI)  : IRequestHandler<SampleRequest , DtoResponse<string>>
{
    public async Task<DtoResponse<string>> Handle(SampleRequest request, CancellationToken cancellationToken)
    {
       return await Task.FromResult(new DtoResponse<string> {Data = "Jub don successfully!"});
    }
}