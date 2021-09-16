using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.Models
{
    public class Employee
    {
        public Employee()
        {
            DirectReportees = new HashSet<Employee>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string IdentityId { get; set; }

        public int Designation { get; set; }

        public int Department { get; set; }

        public int? ReportsTo { get; set; }

        public int? MultiplexId { get; set; }

        public TimeSpan? ShiftLogIn { get; set; }

        public TimeSpan? ShiftLogOut { get; set; }

        // navigation property
        public AppUser Identity { get; set; }

        [ForeignKey("Designation")]
        public EmployeeDesignation EmployeeDesignation { get; set; }

        [ForeignKey("Department")]
        public Department EmployeeDepartment { get; set; }

        [ForeignKey("ReportsTo")]
        public virtual Employee EmployeeReportsTo { get; set; }

        [ForeignKey("Domain")]
        public Domain EmployeeDomain { get; set; }

        public virtual ICollection<Employee> DirectReportees { get; set; }

    }
}