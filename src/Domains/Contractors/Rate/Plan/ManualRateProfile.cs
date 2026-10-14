namespace Acme.Workforce.Contractors.Rate.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual rate profile is in its life cycle.</summary>
public enum ManualRateProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a manual rate profile and its current status.</summary>
public sealed class ManualRateProfile(string code, string label, int limit, int tenureMonths, int version, double discount, string name, int windowDays, ManualRateProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Version { get; set; } = version;
    public double Discount { get; set; } = discount;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public ManualRateProfileStatus Status { get; set; } = status;

    /// <summary>Starts a manual rate profile in status requested.</summary>
    public static ManualRateProfile NewManualRateProfile(string code, string label)
    {
        return new ManualRateProfile(code, label, 0, 0, 0, 0.0, "", 0, ManualRateProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualRateProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualRateProfileStatus next)
    {
        switch (Status)
        {
            case ManualRateProfileStatus.Requested:
                switch (next)
                {
                    case ManualRateProfileStatus.Revoked:
                        return true;
                }
                return false;
            case ManualRateProfileStatus.Granted:
                switch (next)
                {
                    case ManualRateProfileStatus.Revoked:
                    case ManualRateProfileStatus.Expired:
                        return true;
                }
                return false;
            case ManualRateProfileStatus.Revoked:
                switch (next)
                {
                    case ManualRateProfileStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualRateProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual rate profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"manual rate profile"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Discount + (double)Version, 2);
    }

    public ManualRateProfile Copy()
    {
        return new ManualRateProfile(Code, Label, Limit, TenureMonths, Version, Discount, Name, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
