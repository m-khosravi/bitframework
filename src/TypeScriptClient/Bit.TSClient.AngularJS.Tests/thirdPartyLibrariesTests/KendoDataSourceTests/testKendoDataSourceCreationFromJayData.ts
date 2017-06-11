﻿let testKendoDataSourceCreationFromJayDataEntitySet = async (): Promise<void> => {
    const contextProvider = Bit.DependencyManager.getCurrent().resolveObject<Bit.Contracts.IEntityContextProvider>("EntityContextProvider");
    const context = await contextProvider.getContext<TestContext>("Test");
    const dataSource = context.testModels.asKendoDataSource();
    await dataSource.query({
        filter: { field: "StringProperty", operator: "eq", value: "Test" }
    });

    if (dataSource.dataView().length != 1) {
        throw new Error("kendo dataSource has a problem in query method");
    }
};

let testKendoDataSourceCreationFromJayDataODataFunctionCall = async (): Promise<void> => {
    const contextProvider = Bit.DependencyManager.getCurrent().resolveObject<Bit.Contracts.IEntityContextProvider>("EntityContextProvider");
    const context = await contextProvider.getContext<TestContext>("Test");
    const dataSource = context.testModels.getTestModelsByStringPropertyValue("1").asKendoDataSource();
    await dataSource.query({
        filter: { field: "StringProperty", operator: "eq", value: "String1" }
    });

    if (dataSource.dataView().length != 1) {
        throw new Error("kendo dataSource has a problem in query method");
    }
};