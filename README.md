# Connecting Databases to Syncfusion ASP.NET MVC Grid

## Project Description

A suite of projects that enable data binding and CRUD action handling in the Syncfusion ASP.NET MVC Grid from various databases using CustomAdaptor and UrlAdaptor.

## Project Overview

The **connecting-databases-to-aspnetmvc-grid** repository provides a collection of focused examples designed to help developers understand different data binding and integration patterns supported by the Syncfusion EJ2 ASP.NET MVC Grid. Each sample is organized by scenario, making it easier to explore and learn specific approaches without unnecessary complexity.

The samples primarily focus on connecting the Grid component with databases, adapters, and backend services commonly used in modern web applications. These examples are intended for developers who want practical references while building enterprise-grade  applications using Syncfusion components.

## Using UrlAdaptor

The [UrlAdaptor](https://ej2.syncfusion.com/aspnetmvc/documentation/grid/connecting-to-adaptors/url-adaptor) serves as the base adaptor for facilitating communication between remote data services and a UI component. It enables the remote binding of data to the Syncfusion Angular Grid by connecting to an existing, pre-configured API service linked to the Microsoft SQL Server database. While the Grid supports various adaptors to fulfill this requirement, including [Web API](https://ej2.syncfusion.com/aspnetmvc/documentation/grid/connecting-to-adaptors/web-api-adaptor), [ODataV4](https://ej2.syncfusion.com/aspnetmvc/documentation/grid/connecting-to-adaptors/odatav4-adaptor), [UrlAdaptor](https://ej2.syncfusion.com/aspnetmvc/documentation/grid/connecting-to-adaptors/url-adaptor), and [Web Method](https://ej2.syncfusion.com/aspnetmvc/documentation/grid/connecting-to-adaptors/web-method-adaptor), the `UrlAdaptor` is particularly useful for scenarios where a custom API service with unique logic for handling data and CRUD operations is in place. This approach allows for custom handling of data and CRUD operations, and the resultant data is returned in the `result` and `count` format for display in the Grid.

## Using CustomAdaptor

The [CustomAdaptor](https://ej2.syncfusion.com/aspnetmvc/documentation/grid/connecting-to-adaptors/custom-adaptor) serves as a mediator between the UI component and the database for data binding. While the data source from the database can be directly bound to the Syncfusion Angular Grid locally using the `dataSource` property, the `CustomAdaptor` approach is preferred as it allows for customization of both data operations and CRUD operations according to specific requirements. In this approach, for every action in the Grid, a corresponding request with action details is sent to the `CustomAdaptor`. The Grid provides predefined methods to perform data operations such as **searching**, **filtering**, **sorting**, **aggregation**, **paging**, and **grouping**. Alternatively, your own custom methods can be employed to execute operations and return the data in the `result` and `count` format for displaying in the Grid. Additionally, for CRUD operations, predefined methods can be overridden to provide custom functionality. Further details on this can be found in the later part of the documentation.
