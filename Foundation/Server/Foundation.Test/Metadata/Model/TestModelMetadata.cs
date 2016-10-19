﻿using Foundation.Api.Contracts.Metadata;
using Foundation.Api.Implementations.Metadata;
using Foundation.Test.Model.DomainModels;
using System.Collections.Generic;

namespace Foundation.Test.Metadata.Model
{
    public class TestModelMetadata : DefaultDtoMetadataBuilder<TestModel>
    {
        public override IEnumerable<ObjectMetadata> BuildMetadata()
        {
            AddDtoMetadata(new DtoMetadata { });

            AddMemberMetadata(nameof(TestModel.Id), new DtoMemberMetadata
            {

            });

            AddMemberMetadata(nameof(TestModel.StringProperty), new DtoMemberMetadata
            {

            });

            AddMemberMetadata(nameof(TestModel.DateProperty), new DtoMemberMetadata
            {

            });

            AddMemberMetadata(nameof(TestModel.Version), new DtoMemberMetadata
            {

            });

            return base.BuildMetadata();
        }
    }
}
