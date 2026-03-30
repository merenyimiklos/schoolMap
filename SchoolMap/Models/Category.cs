namespace SchoolMap.Models
{
    /// <summary>
    /// Kategória típusok a helyiségek csoportosításához.
    /// Új kategória hozzáadásához egyszerűen bővítsd ezt az enum-ot.
    /// </summary>
    public enum Category
    {
        Tanterem,       // Általános tantermek
        Iroda,          // Igazgatóság, titkárság, tanári
        KozossegiTer,   // Aula, ebédlő, büfé
        Mosdo,          // Mosdók
        SpecialisTer,   // Labor, tornaterem, könyvtár
        Egyeb           // Minden egyéb
    }
}
