namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict tax form bundle is in its life cycle.</summary>
public enum StrictTaxFormBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A strict tax form bundle. Its code is unique within a tenant.</summary>
public sealed class StrictTaxFormBundle(string code, string name, bool archived, int tenureMonths, int windowDays, string team, StrictTaxFormBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public int TenureMonths { get; set; } = tenureMonths;
    public int WindowDays { get; set; } = windowDays;
    public string Team { get; set; } = team;
    public StrictTaxFormBundleStatus Status { get; set; } = status;

    /// <summary>Starts a strict tax form bundle in status idle.</summary>
    public static StrictTaxFormBundle NewStrictTaxFormBundle(string code, string name)
    {
        return new StrictTaxFormBundle(code, name, false, 0, 0, "", StrictTaxFormBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictTaxFormBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictTaxFormBundleStatus next)
    {
        switch (Status)
        {
            case StrictTaxFormBundleStatus.Idle:
                return next == StrictTaxFormBundleStatus.Ready || next == StrictTaxFormBundleStatus.Stopped;
            case StrictTaxFormBundleStatus.Warming:
                return next == StrictTaxFormBundleStatus.Ready || next == StrictTaxFormBundleStatus.Draining;
            case StrictTaxFormBundleStatus.Ready:
                return next == StrictTaxFormBundleStatus.Draining || next == StrictTaxFormBundleStatus.Stopped;
            case StrictTaxFormBundleStatus.Draining:
                switch (next)
                {
                    case StrictTaxFormBundleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictTaxFormBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict tax form bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)TenureMonths - (double)WindowDays, 2);
    }

    public StrictTaxFormBundle Copy()
    {
        return new StrictTaxFormBundle(Code, Name, Archived, TenureMonths, WindowDays, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
