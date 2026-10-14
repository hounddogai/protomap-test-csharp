namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance preference is in its life cycle.</summary>
public enum BalancePreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One balance preference, with what the planners need to act on it.</summary>
public sealed class BalancePreference(string code, string title, bool pinned, double rating, double floorValue, double hours, double margin, string currency, string unit, double score, int updatedAt, BalancePreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public double Rating { get; set; } = rating;
    public double FloorValue { get; set; } = floorValue;
    public double Hours { get; set; } = hours;
    public double Margin { get; set; } = margin;
    public string Currency { get; set; } = currency;
    public string Unit { get; set; } = unit;
    public double Score { get; set; } = score;
    public int UpdatedAt { get; set; } = updatedAt;
    public BalancePreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a balance preference in status open.</summary>
    public static BalancePreference NewBalancePreference(string code, string title)
    {
        return new BalancePreference(
            code,
            title,
            false,
            0.0,
            0.0,
            0.0,
            0.0,
            "",
            "",
            0.0,
            0,
            BalancePreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalancePreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalancePreferenceStatus next)
    {
        switch (Status)
        {
            case BalancePreferenceStatus.Open:
                return next == BalancePreferenceStatus.Resolved || next == BalancePreferenceStatus.Reopened;
            case BalancePreferenceStatus.InReview:
                switch (next)
                {
                    case BalancePreferenceStatus.Reopened:
                        return true;
                }
                return false;
            case BalancePreferenceStatus.Resolved:
                switch (next)
                {
                    case BalancePreferenceStatus.Reopened:
                    case BalancePreferenceStatus.Archived:
                        return true;
                }
                return false;
            case BalancePreferenceStatus.Reopened:
                return next == BalancePreferenceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BalancePreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Margin * Score, 2);
    }

    public BalancePreference Copy()
    {
        return new BalancePreference(
            Code,
            Title,
            Pinned,
            Rating,
            FloorValue,
            Hours,
            Margin,
            Currency,
            Unit,
            Score,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
