public interface IDataService
{
    Task InitAsync();
    Task<IEnumerable<Trainingsplan>> LadeTrainingsplaeneAsync();
    Task SpeichereTrainingsplanAsync(Trainingsplan plan, IEnumerable<Tag> tage);
    Task LoescheTrainingsplanAsync(Trainingsplan plan);

    Task<List<Uebung>> LadeUebungenZuPlanAsync(int trainingsplanId);
    Task SpeichereUebung(Uebung uebung);
    Task SpeichereTagAsync(Tag tag);
    Task DeleteTagAsync(int tagId);
    Task<List<Tag>> LadeTageAsync(int trainingsplanId);

    Task<List<Satz>> LadeSaetzeFuerUebungAsync(int uebungId);
    Task SpeichereSaetzeFuerUebungAsync(int uebungId, IEnumerable<Satz> saetze);
    Task<List<Satz>> LadeSaetzeFuerUebungInWocheAsync(int uebungId, int jahr, int kw);
    Task SpeichereSaetzeFuerUebungInWocheAsync(int uebungId, int jahr, int kw, IEnumerable<Satz> saetze);
    Task<List<Satz>> LadeSaetzeInWocheAsync(int jahr, int kw);

    Task<List<Uebung>> LadeUebungenByIdsAsync(IEnumerable<int> uebungIds);
    Task<List<Tag>> LadeTagsByIdsAsync(IEnumerable<int> tagIds);
    Task<List<Trainingsplan>> LadePlaeneByIdsAsync(IEnumerable<int> planIds);

    //  NEU:
    Task<int> ImportTrainingsplaeneAsync(IEnumerable<Trainingsplan> plaene);
    Task LoescheTrainingsplanKomplettAsync(int planId);

    Task SpeichereSatzAsync(Satz satz);
    Task LoescheSatzAsync(int satzId);    // TODO
}
