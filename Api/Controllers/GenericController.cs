using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Api.Controllers
{
    public class GenericController<T> : BaseController where T : BaseEntity
    {
        private readonly IGenericRepository<T> repo;

        public GenericController(IGenericRepository<T> repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<T>>> Get()
        {
            var getAllResponse = await repo.GetAllAsync();
            return getAllResponse == null ? NotFound() : Ok(getAllResponse);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<T>> Get(int id)
        {
            var getResponse = await repo.GetByIdAsync(id);
            return getResponse == null ? NotFound() : Ok(getResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(T entity)
        {
            try
            {
                await repo.UpdateAsync(entity);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<T>> Post(T entity)
        {
            try
            {
                await repo.CreateAsync(entity);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) { return NotFound(); }

            try
            {
                await repo.DeleteAsync(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
