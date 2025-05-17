public partial class TypesUtils
{
    public static string getTypeName(int typeId)
    {
        var type_name = "";

        switch (typeId)
        {
            case 1:
                type_name = "normal";
                break;
            case 2:
                type_name = "fighting";
                break;
            case 3:
                type_name = "flying";
                break;
            case 4:
                type_name = "poison";
                break;
            case 5:
                type_name = "ground";
                break;
            case 6:
                type_name = "rock";
                break;
            case 7:
                type_name = "bug";
                break;
            case 8:
                type_name = "ghost";
                break;
            case 9:
                type_name = "steel";
                break;
            case 10:
                type_name = "fire";
                break;
            case 11:
                type_name = "water";
                break;
            case 12:
                type_name = "grass";
                break;
            case 13:
                type_name = "electric";
                break;
            case 14:
                type_name = "psychic";
                break;
            case 15:
                type_name = "ice";
                break;
            case 16:
                type_name = "dragon";
                break;
            case 17:
                type_name = "dark";
                break;
            case 18:
                type_name = "fairy";
                break;
        }

        return type_name;
    }
}