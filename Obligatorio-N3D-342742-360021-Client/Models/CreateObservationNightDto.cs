namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public record CreateObservationNightDto(
        int RequestingUserId,
        int CelestialObjectId,
        string Date,
        string Details
    );
}
