namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model preference is in its life cycle.</summary>
public enum CostModelPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a cost model preference and its current status.</summary>
public sealed class CostModelPreference(string code, string title, string phone, string salaryBand, string checksum, int updatedAt, string label, double rating, int retries, string region, CostModelPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public string SalaryBand { get; set; } = salaryBand;
    public string Checksum { get; set; } = checksum;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public int Retries { get; set; } = retries;
    public string Region { get; set; } = region;
    public CostModelPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a cost model preference in status new.</summary>
    public static CostModelPreference NewCostModelPreference(string code, string title)
    {
        return new CostModelPreference(code, title, "", "", "", 0, "", 0.0, 0, "", CostModelPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelPreferenceStatus next)
    {
        switch (Status)
        {
            case CostModelPreferenceStatus.New:
                return next == CostModelPreferenceStatus.Failed;
            case CostModelPreferenceStatus.Queued:
                switch (next)
                {
                    case CostModelPreferenceStatus.Done:
                    case CostModelPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case CostModelPreferenceStatus.Running:
                switch (next)
                {
                    case CostModelPreferenceStatus.Done:
                        return true;
                }
                return false;
            case CostModelPreferenceStatus.Done:
                switch (next)
                {
                    case CostModelPreferenceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Retries + (double)UpdatedAt, 2);
    }

    public CostModelPreference Copy()
    {
        return new CostModelPreference(
            Code,
            Title,
            Phone,
            SalaryBand,
            Checksum,
            UpdatedAt,
            Label,
            Rating,
            Retries,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
