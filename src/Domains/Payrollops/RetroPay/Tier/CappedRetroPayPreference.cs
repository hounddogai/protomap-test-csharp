namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped retro pay preference is in its life cycle.</summary>
public enum CappedRetroPayPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A capped retro pay preference as the retro pay team keeps it.</summary>
public sealed class CappedRetroPayPreference(string code, string label, string name, string country, string source, string employeeId, int graceDays, CappedRetroPayPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Source { get; set; } = source;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public CappedRetroPayPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a capped retro pay preference in status created.</summary>
    public static CappedRetroPayPreference NewCappedRetroPayPreference(string code, string label)
    {
        return new CappedRetroPayPreference(code, label, "", "", "", "", 0, CappedRetroPayPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRetroPayPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRetroPayPreferenceStatus next)
    {
        switch (Status)
        {
            case CappedRetroPayPreferenceStatus.Created:
                return next == CappedRetroPayPreferenceStatus.Validated || next == CappedRetroPayPreferenceStatus.Published;
            case CappedRetroPayPreferenceStatus.Validated:
                switch (next)
                {
                    case CappedRetroPayPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case CappedRetroPayPreferenceStatus.Published:
                return next == CappedRetroPayPreferenceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CappedRetroPayPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped retro pay preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)GraceDays;
    }

    public CappedRetroPayPreference Copy()
    {
        return new CappedRetroPayPreference(Code, Label, Name, Country, Source, EmployeeId, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
