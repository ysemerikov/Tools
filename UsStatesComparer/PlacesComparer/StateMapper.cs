namespace UsStatesComparer.PlacesComparer;

public static class StateMapper
{
    public static readonly (string Name, State Value)[] AllStates =
        Enum.GetValues<State>()
            .Select(x => (x.ToString("G").ToLowerInvariant(), x))
            .OrderBy(x => x.Item1)
            .ToArray();

    private static readonly Dictionary<string, State> Predefined = new(StringComparer.InvariantCultureIgnoreCase)
    {
        {"D.C.", State.DistrictOfColumbia},
        {"Ala.", State.Alabama},
        {"Fla.", State.Florida},
        {"Ga.", State.Georgia},
        {"Ky.", State.Kentucky},
        {"La.", State.Louisiana},
        {"Md.", State.Maryland},
        {"Mo.", State.Missouri},
        {"Miss.", State.Mississippi},
        {"N.H.", State.NewHampshire},
        {"N.J.", State.NewJersey},
        {"N.M.", State.NewMexico},
        {"N.Y.", State.NewYork},
        {"N.C.", State.NorthCarolina},
        {"N.D.", State.NorthDakota},
        {"Pa.", State.Pennsylvania},
        {"R.I.", State.RhodeIsland},
        {"S.C.", State.SouthCarolina},
        {"S.D.", State.SouthDakota},
        {"Vt.", State.Vermont},
        {"Va.", State.Virginia},
        {"W.Va.", State.WestVirginia},
    };

    public static State FromString(string str)
    {
        if (Predefined.TryGetValue(str, out var r))
        {
            return r;
        }

        var lettersOnly = new string(str.Where(char.IsLetter).Select(char.ToLowerInvariant).ToArray());
        var result = AllStates.Where(x => x.Name.StartsWith(lettersOnly)).Take(2).ToList();
        if (result.Count == 1)
        {
            return result[0].Value;
        }

        throw new ArgumentException($"Can't parse '{str}' to state");
    }
}
