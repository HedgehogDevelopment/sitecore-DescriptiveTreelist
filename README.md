sitecore-DescriptiveTreelist
============================

Sitecore Descriptive Tree List Field Type

This Visual Studio 2010 Solution contains two projects.
1. A Web Application project (DescriptiveTreelist) 
2. A Team Development for Sitecore (TDS) project with the required Sitecore item.

While TDS is not required to compile and use this field, it is recommended and a free trial is available at
http://teamdevelopmentforsitecore.com/

To compile and add the field type to your Sitecore system with TDS:
1. Add your Sitecore DLL's into the .\Lib directory
2. Within the TDS project properties, put in the path to your Sitecore web root + Sitecore URL
3. Deploy the solution

To compile and add the field type to your Sitecore system without TDS:
1. Add your Sitecore DLL's into the .\Lib directory
2. Compile the DescriptiveTreelist project
3. Copy the DescriptiveTreelist\bin\HedgehogDevelopment.SharedSource.DescriptiveTreelist.dll file to your webroot\bin directory
4. Copy the DescriptiveTreelist\App_Config\Include\DescriptiveTreelist.config to your webroot\App_Config\Include directory
5. Within Sitecore go to the /sitecore/system/Field types/List Types item in the 'core' database 
6. On the 'Developer' tab, press the 'Serialize Tree' button for the 'List Types' item
7. Copy the 'sitecore' directory within the DescriptiveTreelist.Core project to the 'Data\serialization\core' directory for your Sitecore site
8. Back in Sitecore... press the "Update Tree" button on the 'List Types' item 

A standard Sitecore package has also been provided.