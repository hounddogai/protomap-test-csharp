namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default license seat profile is in its life cycle.</summary>
public enum DefaultLicenseSeatProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A default license seat profile as the license seat team keeps it.</summary>
public sealed class DefaultLicenseSeatProfile(string code, string title, string source, string location, double threshold, double rate, int capacity, string team, string contactEmail, DefaultLicenseSeatProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Location { get; set; } = location;
    public double Threshold { get; set; } = threshold;
    public double Rate { get; set; } = rate;
    public int Capacity { get; set; } = capacity;
    public string Team { get; set; } = team;
    public string ContactEmail { get; set; } = contactEmail;
    public DefaultLicenseSeatProfileStatus Status { get; set; } = status;

    /// <summary>Starts a default license seat profile in status idle.</summary>
    public static DefaultLicenseSeatProfile NewDefaultLicenseSeatProfile(string code, string title)
    {
        return new DefaultLicenseSeatProfile(
            code,
            title,
            "",
            "",
            0.0,
            0.0,
            0,
            "",
            "",
            DefaultLicenseSeatProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultLicenseSeatProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultLicenseSeatProfileStatus next)
    {
        switch (Status)
        {
            case DefaultLicenseSeatProfileStatus.Idle:
                switch (next)
                {
                    case DefaultLicenseSeatProfileStatus.Draining:
                    case DefaultLicenseSeatProfileStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultLicenseSeatProfileStatus.Warming:
                switch (next)
                {
                    case DefaultLicenseSeatProfileStatus.Draining:
                        return true;
                }
                return false;
            case DefaultLicenseSeatProfileStatus.Ready:
                return next == DefaultLicenseSeatProfileStatus.Draining || next == DefaultLicenseSeatProfileStatus.Stopped;
            case DefaultLicenseSeatProfileStatus.Draining:
                switch (next)
                {
                    case DefaultLicenseSeatProfileStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultLicenseSeatProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default license seat profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default license seat profile"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Capacity - Rate, 2);
    }

    public DefaultLicenseSeatProfile Copy()
    {
        return new DefaultLicenseSeatProfile(
            Code,
            Title,
            Source,
            Location,
            Threshold,
            Rate,
            Capacity,
            Team,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
