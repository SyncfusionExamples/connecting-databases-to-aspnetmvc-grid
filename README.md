# Connecting Database to Syncfusion ASP.NET MVC Grid

This section describes how to connect and retrieve data from a Microsoft SQL Server database using [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient) and bind it to the Syncfusion ASP.NET MVC Grid.

Microsoft SQL Server database can be bound to the Grid in different ways (i.e.) using `DataSource` property, custom adaptor and remote data binding using various adaptors. In this documentation, two approaches will be examined to connect a Microsoft SQL Server database to a Grid. Both the approaches have capability to handle data and CRUD operations with built-in methods as well as can be customized as per your own. 

**1. Using UrlAdaptor**

The [UrlAdaptor](https://ej2.syncfusion.com/aspnetmvc/documentation/grid/connecting-to-adaptors/url-adaptor) serves as the base adaptor for facilitating communication between remote data services and an UI component. It enables the remote binding of data to the Syncfusion ASP.NET MVC Grid by connecting to an existing pre-configured API service linked to the Microsoft SQL Server database. While the Grid supports various adaptors to fulfill this requirement, including [Web API](https://ej2.syncfusion.com/aspnetmvc/documentation/grid/connecting-to-adaptors/web-api-adaptor), [ODataV4](https://ej2.syncfusion.com/aspnetmvc/documentation/grid/connecting-to-adaptors/odatav4-adaptor), [UrlAdaptor](https://ej2.syncfusion.com/aspnetmvc/documentation/grid/connecting-to-adaptors/url-adaptor), and `GraphQL`, the `UrlAdaptor` is particularly useful for the scenarios where a custom API service with unique logic for handling data and CRUD operations is in place. This approach allows for custom handling of data and CRUD operations, and the resultant data returned in the `result` and `count` format for display in the Grid.

**2. Using CustomAdaptor**

The [CustomAdaptor](https://ej2.syncfusion.com/aspnetmvc/documentation/grid/connecting-to-adaptors/custom-adaptor) serves as a mediator between the UI component and the database for data binding. While the data source from the database can be directly bound to the Syncfusion ASP.NET MVC Grid locally using the `DataSource` property, the `CustomAdaptor` approach is preferred as it allows for customization of both data operations and CRUD operations according to specific requirements. In this approach, for every action in the Grid, a corresponding request with action details is sent to the `CustomAdaptor`. The Grid provides predefined methods to perform data operations such as **searching**, **filtering**, **sorting**, **aggregation**, **paging** and **grouping**. Alternatively, your own custom methods can be employed to execute operations and return the data in the `result` and `count` format for displaying in the Grid. Additionally, for CRUD operations, predefined methods can be overridden to provide custom functionality. Further details on this can be found in the later part of the documentation.

Here, we have explained in detail how to bind the listed databases below to the Blazor DataGrid component using CustomAdaptor and UrlAdaptor.

1. Microsoft SQL Server Database
2. MYSQL Database
3. PostgreSQL Database
4. Dapper
5. SQLite
6. Entity Framework