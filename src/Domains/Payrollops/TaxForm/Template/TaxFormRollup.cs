namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form rollup is in its life cycle.</summary>
public enum TaxFormRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One tax form rollup, with what the planners need to act on it.</summary>
public sealed class TaxFormRollup(string code, string title, string externalRef, double ratio, int revision, bool pinned, int graceDays, TaxFormRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public double Ratio { get; set; } = ratio;
    public int Revision { get; set; } = revision;
    public bool Pinned { get; set; } = pinned;
    public int GraceDays { get; set; } = graceDays;
    public TaxFormRollupStatus Status { get; set; } = status;

    /// <summary>Starts a tax form rollup in status created.</summary>
    public static TaxFormRollup NewTaxFormRollup(string code, string title)
    {
        return new TaxFormRollup(code, title, "", 0.0, 0, false, 0, TaxFormRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormRollupStatus next)
    {
        switch (Status)
        {
            case TaxFormRollupStatus.Created:
                switch (next)
                {
                    case TaxFormRollupStatus.Retired:
                        return true;
                }
                return false;
            case TaxFormRollupStatus.Validated:
                switch (next)
                {
                    case TaxFormRollupStatus.Published:
                    case TaxFormRollupStatus.Retired:
                        return true;
                }
                return false;
            case TaxFormRollupStatus.Published:
                switch (next)
                {
                    case TaxFormRollupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetVolume()
    {
        return Math.Round(Ratio - (double)GraceDays, 2);
    }

    public TaxFormRollup Copy()
    {
        return new TaxFormRollup(Code, Title, ExternalRef, Ratio, Revision, Pinned, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
