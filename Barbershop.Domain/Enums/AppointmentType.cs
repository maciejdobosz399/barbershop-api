namespace Barbershop.Domain.Enums;

public enum AppointmentType
{
    Haircut,
    Shave,
    HaircutAndShave
}

public static class AppointmentTypeExtensions
{
    public static TimeSpan GetDuration(this AppointmentType appointmentType) => appointmentType switch
    {
        AppointmentType.Haircut => TimeSpan.FromMinutes(30),
        AppointmentType.Shave => TimeSpan.FromMinutes(15),
        AppointmentType.HaircutAndShave => TimeSpan.FromMinutes(45),
        _ => throw new ArgumentOutOfRangeException(nameof(appointmentType), appointmentType, null)
    };
}