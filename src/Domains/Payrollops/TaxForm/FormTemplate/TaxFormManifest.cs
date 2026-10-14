namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form manifest is in its life cycle.</summary>
public enum TaxFormManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One tax form manifest, with what the planners need to act on it.</summary>
public sealed class TaxFormManifest(string code, string title, string reviewer, string summary, string checksum, double score, TaxFormManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Summary { get; set; } = summary;
    public string Checksum { get; set; } = checksum;
    public double Score { get; set; } = score;
    public TaxFormManifestStatus Status { get; set; } = status;

    /// <summary>Starts a tax form manifest in status open.</summary>
    public static TaxFormManifest NewTaxFormManifest(string code, string title)
    {
        return new TaxFormManifest(code, title, "", "", "", 0.0, TaxFormManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormManifestStatus next)
    {
        switch (Status)
        {
            case TaxFormManifestStatus.Open:
                switch (next)
                {
                    case TaxFormManifestStatus.InReview:
                    case TaxFormManifestStatus.Resolved:
                        return true;
                }
                return false;
            case TaxFormManifestStatus.InReview:
                switch (next)
                {
                    case TaxFormManifestStatus.Resolved:
                        return true;
                }
                return false;
            case TaxFormManifestStatus.Resolved:
                switch (next)
                {
                    case TaxFormManifestStatus.Reopened:
                    case TaxFormManifestStatus.Archived:
                        return true;
                }
                return false;
            case TaxFormManifestStatus.Reopened:
                switch (next)
                {
                    case TaxFormManifestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public TaxFormManifest Copy()
    {
        return new TaxFormManifest(Code, Title, Reviewer, Summary, Checksum, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
