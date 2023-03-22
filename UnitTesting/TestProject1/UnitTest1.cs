using IniConfigDotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IniConfigDotNetUnitTests
{
    [TestClass]
    public class InMemoryOperations
    {
        const string basesection_name = "Section";
        const string baseentry_name = "Entry";
        const string baseentry_val = "value";


        internal void PopulateIniDemo(IniBase Demo, uint count)
        {
            GenerateDataSections(Demo, count);
            GenerateDataEntries(Demo, count);
            
        }
        /// <summary>
        /// used when we need to generate random section names
        /// </summary>
        /// <param name="Demo"></param>
        /// <param name="count"></param>
        internal void GenerateDataSections(IniBase Demo, uint count)
        {
            
            
            Random random= new Random();
            for (uint step = 0; step < count; step++)
            {
                Demo.CreateSection(basesection_name + random.NextInt64().ToString());
            }
        }

        internal void GenerateDataEntries(IniBase Demo, uint count)
        {
            Random random = new Random(); 
            foreach (string Section in Demo.GetSectionNames())
            {
                for (uint step = 0; step < count; step++) 
                { 
                Demo[Section].Add(baseentry_name + random.NextInt64().ToString(), baseentry_val + random.NextInt64().ToString());
                }
            }
        }
        [TestMethod]
        public void SettingSectionData_notexisting_before_DONOT_CREATE_SECTION()
        {
            IniBase Demo = new IniBase();
            bool first_pass = false;
            try
            {
                Demo.SetSectionEntry("NewSection", "NewEntry", "NewValue", false);
            }
            catch (SectionNotFoundException)
            {
                first_pass = true;
            }

            Assert.IsTrue(first_pass);
        }

        [TestMethod]
        public void SettingSectionData_notexisting_before_CREATE_AS_NEEDED()
        {
            IniBase Demo = new IniBase();
            Demo.SetSectionEntry("NewSection", "NewEntry", "NewValue", true);
            var val = Demo.GetSectionData("NewSection")["NewEntry"];
            var val2 = Demo["NewSection"]["NewEntry"];
            Assert.IsTrue(val == "NewValue");
        }

        [TestMethod]
        public void GettingSectionData_as_array()
        {
            uint section_count = 25500;
            // first populate
            IniBase Demo = new IniBase();
            GenerateDataSections(Demo, section_count);

            var Sections = Demo.GetSectionNames();
            Assert.IsTrue(Sections.Length == section_count);
        }

        [TestMethod]
        public void GettingSectionData_as_list()
        {
            uint section_count = 25500;
            // first populate
            IniBase Demo = new IniBase();
            GenerateDataSections(Demo, section_count);

            var Sections = Demo.GetSectionNamesAsList(false);
            Assert.IsTrue(Sections.Count == section_count);
        }

        [TestMethod]
        public void Getting_SectionData_with_braket_operator()
        {
            IniBase Demo = new IniBase();
            Demo.SetSectionEntry("TestSection", "NONAME", "NODATA");

            var TestMe = Demo["TestSection"];
            Assert.IsTrue(TestMe.GetType() == typeof(Dictionary<string, string>));

            var TestMe2 = Demo["TestSection"]["NONAME"];
            Assert.IsTrue(TestMe2 == "NODATA");
        }
        [TestMethod]
        public void setting_SectionData_with_braket_operator_flex()
        {
            const string Entry = "NewEntry";
            const string Val = "newVal";
            const string Section = "SectionNew";
            IniBase Demo = new();
            Demo.AllowFlexibleIndex= true;
            Demo[Section][Val] = Entry;
            var val = Demo[Section][Val];
            Assert.IsTrue(val == Entry);
        }


        [TestMethod]
        public void setting_SectionData_with_braket_operator_noflex()
        {
            bool Yay = false;
            const string Entry = "NewEntry";
            const string Val = "newVal";
            const string Section = "SectionNew";
            IniBase Demo = new();
            Demo.AllowFlexibleIndex = false;

            try
            {
                Demo[Section][Val] = Entry;
            }
            catch (SectionNotFoundException)
            {
                Yay = true;
            }

            Assert.IsTrue(Yay);
            
        }

        public void SimpleFormatLoadTest()
        {
            IniBase Demo = new IniBase();
            Demo.FileFormatControl = IniBase.FileFormat.Simple;
            Demo.AllowFlexibleIndex = true;

            Demo["COMMENT_TEST"]["COMMENT12"] = "//This is a commennt";
            PopulateIniDemo(Demo, 2);
            string data = Demo.ToString();

            Demo = new IniBase(); 

            

        }
        [TestMethod]
        public void SimpleFormatSaveTest_debug_and_look_at_datavar()
        {
            
            IniBase Demo = new IniBase();
            Demo.FileFormatControl = IniBase.FileFormat.Simple;
            Demo.AllowFlexibleIndex = true;

            Demo["COMMENT_TEST"]["COMMENT12"] = "//This is a commennt";
            PopulateIniDemo(Demo,2);

            

            


            string data = Demo.ToString();


            return;
        }
    }
}