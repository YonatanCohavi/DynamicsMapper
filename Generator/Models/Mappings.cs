using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicsMapper.Models
{
    internal struct Mappings
    {
        public Mappings(string toModel, string toEntity)
        {
            ToEntity = toEntity;
            ToModel = toModel;
        }

        public string ToEntity { get; set; }
        public string ToModel { get; set; }
    }
}
