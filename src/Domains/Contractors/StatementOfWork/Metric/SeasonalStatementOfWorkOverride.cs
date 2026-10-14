namespace Acme.Workforce.Contractors.StatementOfWork.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal statement of work override is in its life cycle.</summary>
public enum SeasonalStatementOfWorkOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal statement of work override. Its code is unique within a tenant.</summary>
public sealed class SeasonalStatementOfWorkOverride(string code, string title, Dictionary<string, string> attributes, int batchSize, int endsAt, string externalRef, double ratio, string jobCode, double amount, string timezone, bool visible, SeasonalStatementOfWorkOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int BatchSize { get; set; } = batchSize;
    public int EndsAt { get; set; } = endsAt;
    public string ExternalRef { get; set; } = externalRef;
    public double Ratio { get; set; } = ratio;
    public string JobCode { get; set; } = jobCode;
    public double Amount { get; set; } = amount;
    public string Timezone { get; set; } = timezone;
    public bool Visible { get; set; } = visible;
    public SeasonalStatementOfWorkOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal statement of work override in status planned.</summary>
    public static SeasonalStatementOfWorkOverride NewSeasonalStatementOfWorkOverride(string code, string title)
    {
        return new SeasonalStatementOfWorkOverride(
            code,
            title,
            new Dictionary<string, string>(),
            0,
            0,
            "",
            0.0,
            "",
            0.0,
            "",
            false,
            SeasonalStatementOfWorkOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalStatementOfWorkOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalStatementOfWorkOverrideStatus next)
    {
        switch (Status)
        {
            case SeasonalStatementOfWorkOverrideStatus.Planned:
                return next == SeasonalStatementOfWorkOverrideStatus.InProgress || next == SeasonalStatementOfWorkOverrideStatus.Completed;
            case SeasonalStatementOfWorkOverrideStatus.Scheduled:
                return next == SeasonalStatementOfWorkOverrideStatus.Cancelled;
            case SeasonalStatementOfWorkOverrideStatus.InProgress:
                return next == SeasonalStatementOfWorkOverrideStatus.Completed || next == SeasonalStatementOfWorkOverrideStatus.Cancelled;
            case SeasonalStatementOfWorkOverrideStatus.Completed:
                switch (next)
                {
                    case SeasonalStatementOfWorkOverrideStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalStatementOfWorkOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal statement of work override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double NetVolume()
    {
        return Math.Round(Amount * (double)EndsAt, 2);
    }

    public SeasonalStatementOfWorkOverride Copy()
    {
        return new SeasonalStatementOfWorkOverride(
            Code,
            Title,
            Attributes,
            BatchSize,
            EndsAt,
            ExternalRef,
            Ratio,
            JobCode,
            Amount,
            Timezone,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
