namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual sentiment adjustment is in its life cycle.</summary>
public enum ManualSentimentAdjustmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual sentiment adjustment. Its code is unique within a tenant.</summary>
public sealed class ManualSentimentAdjustment(string code, string name, double rating, string skill, double minimum, int quantity, ManualSentimentAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public string Skill { get; set; } = skill;
    public double Minimum { get; set; } = minimum;
    public int Quantity { get; set; } = quantity;
    public ManualSentimentAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a manual sentiment adjustment in status created.</summary>
    public static ManualSentimentAdjustment NewManualSentimentAdjustment(string code, string name)
    {
        return new ManualSentimentAdjustment(code, name, 0.0, "", 0.0, 0, ManualSentimentAdjustmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualSentimentAdjustmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualSentimentAdjustmentStatus next)
    {
        switch (Status)
        {
            case ManualSentimentAdjustmentStatus.Created:
                switch (next)
                {
                    case ManualSentimentAdjustmentStatus.Published:
                    case ManualSentimentAdjustmentStatus.Retired:
                        return true;
                }
                return false;
            case ManualSentimentAdjustmentStatus.Validated:
                switch (next)
                {
                    case ManualSentimentAdjustmentStatus.Published:
                    case ManualSentimentAdjustmentStatus.Retired:
                        return true;
                }
                return false;
            case ManualSentimentAdjustmentStatus.Published:
                switch (next)
                {
                    case ManualSentimentAdjustmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualSentimentAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual sentiment adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual sentiment adjustment"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Minimum * (double)Quantity, 2);
    }

    public ManualSentimentAdjustment Copy()
    {
        return new ManualSentimentAdjustment(Code, Name, Rating, Skill, Minimum, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
