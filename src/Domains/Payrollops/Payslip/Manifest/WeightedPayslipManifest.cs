namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted payslip manifest is in its life cycle.</summary>
public enum WeightedPayslipManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A weighted payslip manifest. Its code is unique within a tenant.</summary>
public sealed class WeightedPayslipManifest(string code, string name, List<string> tags, int batchSize, double discount, double fte, int attempts, double hours, string locale, WeightedPayslipManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public int BatchSize { get; set; } = batchSize;
    public double Discount { get; set; } = discount;
    public double Fte { get; set; } = fte;
    public int Attempts { get; set; } = attempts;
    public double Hours { get; set; } = hours;
    public string Locale { get; set; } = locale;
    public WeightedPayslipManifestStatus Status { get; set; } = status;

    /// <summary>Starts a weighted payslip manifest in status open.</summary>
    public static WeightedPayslipManifest NewWeightedPayslipManifest(string code, string name)
    {
        return new WeightedPayslipManifest(
            code,
            name,
            new List<string>(),
            0,
            0.0,
            0.0,
            0,
            0.0,
            "",
            WeightedPayslipManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPayslipManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPayslipManifestStatus next)
    {
        switch (Status)
        {
            case WeightedPayslipManifestStatus.Open:
                switch (next)
                {
                    case WeightedPayslipManifestStatus.InReview:
                        return true;
                }
                return false;
            case WeightedPayslipManifestStatus.InReview:
                return next == WeightedPayslipManifestStatus.Resolved || next == WeightedPayslipManifestStatus.Reopened;
            case WeightedPayslipManifestStatus.Resolved:
                switch (next)
                {
                    case WeightedPayslipManifestStatus.Reopened:
                    case WeightedPayslipManifestStatus.Archived:
                        return true;
                }
                return false;
            case WeightedPayslipManifestStatus.Reopened:
                return next == WeightedPayslipManifestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedPayslipManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted payslip manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetVolume()
    {
        return Math.Round(Hours * (double)Attempts, 2);
    }

    public WeightedPayslipManifest Copy()
    {
        return new WeightedPayslipManifest(
            Code,
            Name,
            Tags,
            BatchSize,
            Discount,
            Fte,
            Attempts,
            Hours,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
