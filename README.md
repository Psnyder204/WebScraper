# WebScraper

Basic Web Scraper in C#

Web scraping is a popular and widely used technique for extracting data from websites. It enables developers to automate the process of data extraction and saves them time and effort. In this article, we will be discussing a C# code that performs web scraping to extract information about US embassies.

Functionality and Usefulness

The code consists of three main parts:

ScrapeEmbassyInfo method
AddEmbassy method
EmbassyUrlAddRequest class

The ScrapeEmbassyInfo method takes a URL as a parameter and performs web scraping to extract information about US embassies. This method uses the HtmlWeb class from the HtmlAgilityPack library to load the HTML document from the specified URL. Once the document is loaded, the method uses the SelectNodes method to select specific elements from the HTML document based on their attributes.

In this case, the method is searching for elements that start with a specific string in their href attribute. The selected elements are then converted into a list of links, which are used to load the pages for each embassy.

The AddEmbassy method uses ADO.NET to execute a stored procedure named EmbassyContact_Insert which inserts the embassy information into the database. It passes in the values of the EmbassyUrlAddRequest model as parameters to the stored procedure. The Id of the inserted record is returned as the output parameter.

The EmbassyUrlAddRequest class is a model that represents the information about each embassy. This class contains four properties: Name, Phone, Email, and Website. These properties are used to store the information about each embassy that is extracted from the web pages.
