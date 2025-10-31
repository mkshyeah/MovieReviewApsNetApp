namespace MovieRes.Core.EndPoints;

public interface IEndPoint
{
    void MapEndPoint(IEndpointRouteBuilder app);
}