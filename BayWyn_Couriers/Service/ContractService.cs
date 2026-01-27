using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BayWyn_Couriers.Models;

namespace BayWyn_Couriers.Service
{
    internal class ContractService
    {
        // Simple test code to create a new contract
        public void NewContract(string companyName)
        {
            // Method to create a new contract for the client
            // Implementation can be added in the future
            Contract newContract = new Contract();
            newContract.CompanyName = companyName;
        }

    }
}
