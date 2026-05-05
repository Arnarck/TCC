using System.Collections.Generic;
public interface ITrioRule
{
    bool IsValid(Card a, Card b, Card c);
    int CalculateScore(Card a, Card b, Card c);
}
public class SameTypeRule : ITrioRule
{
    public bool IsValid(Card a, Card b, Card c)
    {
        return a.type == b.type && b.type == c.type;
    }

    public int CalculateScore(Card a, Card b, Card c)
    {
        int baseScore = (a.points + a.improvedPoints) + (b.points + b.improvedPoints) + (c.points + c.improvedPoints);
        return baseScore; // PODE ADICIONAR BONUS 
    }
}
public class SameFamilyRule : ITrioRule
{
    public bool IsValid(Card a, Card b, Card c)
    {
        return a.familyType == b.familyType &&
               b.familyType == c.familyType;
    }
      public int CalculateScore(Card a, Card b, Card c)
    {
        int baseScore = a.points + b.points + c.points;
        return baseScore; // PODE ADICIONAR BONUS 
    }

}
public class AllDifferentRule : ITrioRule
{
    public bool IsValid(Card a, Card b, Card c)
    {
        return a.type != b.type &&
               a.type != c.type &&
               b.type != c.type;
    }
      public int CalculateScore(Card a, Card b, Card c)
    {
        int baseScore = a.points + b.points + c.points;
        return baseScore; // PODE ADICIONAR BONUS 
    }
}

public class TrioSystem
{
    private List<ITrioRule> rules = new List<ITrioRule>();

    public TrioSystem()
    {
        rules.Add(new SameTypeRule());
        rules.Add(new SameFamilyRule());
        rules.Add(new AllDifferentRule());
       
    }

    public bool TryFindTrio(List<Card> cards, out Card a, out Card b, out Card c)
{
    a = b = c = null;

    for (int i = 0; i < cards.Count; i++)
    {
        for (int j = i + 1; j < cards.Count; j++)
        {
            for (int k = j + 1; k < cards.Count; k++)
            {
                foreach (var rule in rules)
                {
                    if (rule.IsValid(cards[i], cards[j], cards[k]))
                    {
                        a = cards[i];
                        b = cards[j];
                        c = cards[k];
                        return true;
                    }
                }
            }
        }
    }

    return false;
}
   public int CalculateScore(Card a, Card b, Card c)
{
    foreach (var rule in rules)
    {
        if (rule.IsValid(a, b, c))
        {
            return rule.CalculateScore(a, b, c);
        }
    }

    //Debug.Log("Nenhuma regra válida encontrada para esse trio.");
    return 0;
}
}