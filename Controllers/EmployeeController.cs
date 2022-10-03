using DotnetCurd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;
using DotnetCurd.Repository;
using System.Collections.Generic;
using System.Linq;

namespace DotnetCurd.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly InventoryContext _context;
        private readonly IMessagePublisher messagePublisher;

        public EmployeeController(InventoryContext context, IMessagePublisher messagePublisher)
        {
            _context = context;
            this.messagePublisher = messagePublisher;
        }

        // GET: Employee_Controller1
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.ToListAsync();
            return View(employees);
        }

        public async Task<IActionResult> AddOrEdit(int? employeeId)
        {
            ViewBag.PageName = employeeId == null ? "Create Employee" : "Edit Employee";
            ViewBag.IsEdit = employeeId == null ? false : true;
            if (employeeId == null)
            {
                return View();
            }
            else
            {
                var employee = await _context.Employees.FindAsync(employeeId);

                if (employee == null)
                {
                    return NotFound();
                }
                return View(employee);
            }
        }

        //AddOrEdit Post Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int employeeId, [Bind("EmployeeId,Name,Designation,Address,Salary,JoiningDate")]
Employees employeeData)
        {
            bool IsEmployeeExist = false;
            

            Employees employee = await _context.Employees.FindAsync(employeeId);

            if (employee != null)
            {
                IsEmployeeExist = true;
            }
            else
            {
                employee = new Employees();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    employee.Name = employeeData.Name;
                    employee.Designation = employeeData.Designation;
                    employee.Address = employeeData.Address;
                    employee.Salary = employeeData.Salary;
                    employee.JoiningDate = employeeData.JoiningDate;

                    if (IsEmployeeExist)
                    {
                        _context.Update(employee);

                        await _context.SaveChangesAsync();

                        var data = Moldingdata(employee, "Update");
                    }
                    else
                    {
                        _context.Add(employee);

                        await _context.SaveChangesAsync();

                        var data = Moldingdata(employee, "Create");
                    }
                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employeeData);
        }

        // Employee Details
        public async Task<IActionResult> Details(int? employeeId)
        {
            if (employeeId == null)
            {
                return NotFound();
            }
            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.EmployeeId == employeeId);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // GET: Employees/Delete/1
        public async Task<IActionResult> Delete(int? employeeId)
        {
            if (employeeId == null)
            {
                return NotFound();
            }
            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.EmployeeId == employeeId);

            return View(employee);
        }

        // POST: Employees/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int employeeId)
         {
            var employee = await _context.Employees.FindAsync(employeeId);
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            var data = Moldingdata(employee, "Delete");

            return RedirectToAction(nameof(Index));
        }

        public Task Moldingdata(Employees emp, string action)
        {
            List<CustomModel> cmodel = new List<CustomModel>();
            if (action == "Update")
            {
                cmodel.Add(new CustomModel { employee = emp, Action = "Update" });
            }
            else if (action == "Create")
            {
                cmodel.Add(new CustomModel { employee = emp, Action = "Create" });
            }
            else
            {
                cmodel.Add(new CustomModel { employee = emp, Action = "Delete" });
            }
            
            var data = SendMessage(cmodel.FirstOrDefault());
            return Task.CompletedTask;
        }

        public async Task SendMessage(CustomModel emp) 
        {
            await messagePublisher.PublisherAsync(emp);
        }

        
    }
}
