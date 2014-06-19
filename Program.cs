
namespace A2AFragmentFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Iterate each of the Lead assets that have FR Dependent Assets
            ////For each fragment in that article, Go to the English Converted folder and grab its FR.xml
            //////Search if the structure exists in the Intl/Converted/HA.xml file
            //////If it exists...
            ////////it's an unlocalized fragment, so we grab Intl/Converted/FR.xml and replace the identified English FR
            //////If it doesn't exist, that fragment is localized or non-existant, so ignore it and check the next fragment for that article
        }
    }
}
