namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual garnishment baseline is in its life cycle.</summary>
public enum ManualGarnishmentBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A manual garnishment baseline as the garnishment team keeps it.</summary>
public sealed class ManualGarnishmentBaseline(string code, string name, Dictionary<string, string> attributes, double fte, double minimum, double rate, ManualGarnishmentBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Fte { get; set; } = fte;
    public double Minimum { get; set; } = minimum;
    public double Rate { get; set; } = rate;
    public ManualGarnishmentBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a manual garnishment baseline in status requested.</summary>
    public static ManualGarnishmentBaseline NewManualGarnishmentBaseline(string code, string name)
    {
        return new ManualGarnishmentBaseline(
            code,
            name,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            0.0,
            ManualGarnishmentBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualGarnishmentBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualGarnishmentBaselineStatus next)
    {
        switch (Status)
        {
            case ManualGarnishmentBaselineStatus.Requested:
                switch (next)
                {
                    case ManualGarnishmentBaselineStatus.Granted:
                        return true;
                }
                return false;
            case ManualGarnishmentBaselineStatus.Granted:
                return next == ManualGarnishmentBaselineStatus.Revoked;
            case ManualGarnishmentBaselineStatus.Revoked:
                switch (next)
                {
                    case ManualGarnishmentBaselineStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualGarnishmentBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual garnishment baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetTotal()
    {
        return Math.Round(Minimum - Rate, 2);
    }

    public ManualGarnishmentBaseline Copy()
    {
        return new ManualGarnishmentBaseline(Code, Name, Attributes, Fte, Minimum, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
