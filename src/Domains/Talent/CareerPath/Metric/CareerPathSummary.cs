namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path summary is in its life cycle.</summary>
public enum CareerPathSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a career path summary and its current status.</summary>
public sealed class CareerPathSummary(string code, string title, string location, bool pinned, string externalRef, string locale, string phone, int endsAt, CareerPathSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public bool Pinned { get; set; } = pinned;
    public string ExternalRef { get; set; } = externalRef;
    public string Locale { get; set; } = locale;
    public string Phone { get; set; } = phone;
    public int EndsAt { get; set; } = endsAt;
    public CareerPathSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a career path summary in status requested.</summary>
    public static CareerPathSummary NewCareerPathSummary(string code, string title)
    {
        return new CareerPathSummary(code, title, "", false, "", "", "", 0, CareerPathSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathSummaryStatus next)
    {
        switch (Status)
        {
            case CareerPathSummaryStatus.Requested:
                return next == CareerPathSummaryStatus.Granted || next == CareerPathSummaryStatus.Expired;
            case CareerPathSummaryStatus.Granted:
                switch (next)
                {
                    case CareerPathSummaryStatus.Revoked:
                    case CareerPathSummaryStatus.Expired:
                        return true;
                }
                return false;
            case CareerPathSummaryStatus.Revoked:
                switch (next)
                {
                    case CareerPathSummaryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public CareerPathSummary Copy()
    {
        return new CareerPathSummary(Code, Title, Location, Pinned, ExternalRef, Locale, Phone, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
