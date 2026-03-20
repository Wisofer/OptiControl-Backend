namespace OptiControl.Utils;

public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo NicaraguaTimeZone = ResolveNicaraguaTimeZone();

    private static TimeZoneInfo ResolveNicaraguaTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Managua");
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }

    public static DateTime UtcNow() => DateTime.UtcNow;

    public static DateTime NicaraguaNow()
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, NicaraguaTimeZone);

    public static DateTime NicaraguaToday()
        => NicaraguaNow().Date;

    public static DateTime ToNicaragua(DateTime dateTime)
    {
        var utc = dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
        return TimeZoneInfo.ConvertTimeFromUtc(utc, NicaraguaTimeZone);
    }

    public static string NicaraguaIso(DateTime dateTime)
        => ToNicaragua(dateTime).ToString("yyyy-MM-ddTHH:mm:ss");
}
