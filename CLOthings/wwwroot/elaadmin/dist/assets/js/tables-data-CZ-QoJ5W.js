class h{constructor(t,e={}){this.table=document.getElementById(t),this.table&&(this.options={searchable:e.searchable!==!1,sortable:e.sortable!==!1,perPage:e.perPage||10,perPageOptions:e.perPageOptions||[5,10,25,50],...e},this.currentPage=1,this.sortColumn=-1,this.sortDirection="asc",this.searchTerm="",this.init())}init(){this.originalRows=Array.from(this.table.querySelector("tbody").querySelectorAll("tr")),this.filteredRows=[...this.originalRows],this.createWrapper(),this.options.searchable&&this.createSearch(),this.options.perPageOptions&&this.createPerPageSelector(),this.options.sortable&&this.addSorting(),this.createPagination(),this.render()}createWrapper(){const t=document.createElement("div");t.className="datatable-wrapper";const e=document.createElement("div");e.className="d-flex justify-content-between align-items-center mb-3",e.innerHTML=`
      <div class="datatable-left d-flex align-items-center gap-3"></div>
      <div class="datatable-right"></div>
    `;const s=document.createElement("div");s.className="d-flex justify-content-between align-items-center mt-3",s.innerHTML=`
      <div class="datatable-info"></div>
      <div class="datatable-pagination"></div>
    `,this.table.parentNode.insertBefore(t,this.table),t.appendChild(e),t.appendChild(this.table),t.appendChild(s),this.topLeft=e.querySelector(".datatable-left"),this.topRight=e.querySelector(".datatable-right"),this.bottomLeft=s.querySelector(".datatable-info"),this.bottomRight=s.querySelector(".datatable-pagination")}createSearch(){const t=document.createElement("div");t.className="datatable-search",t.innerHTML=`
      <div class="input-group" style="width: 250px;">
        <span class="input-group-text"><i class="fas fa-search"></i></span>
        <input type="text" class="form-control" placeholder="Search...">
      </div>
    `,t.querySelector("input").addEventListener("input",s=>{this.searchTerm=s.target.value.toLowerCase(),this.filterRows(),this.currentPage=1,this.render()}),this.topRight.appendChild(t)}createPerPageSelector(){const t=document.createElement("div");t.className="d-flex align-items-center gap-2",t.innerHTML=`
      <label class="mb-0">Show</label>
      <select class="form-select form-select-sm" style="width: auto;">
        ${this.options.perPageOptions.map(s=>`<option value="${s}" ${s===this.options.perPage?"selected":""}>${s}</option>`).join("")}
      </select>
      <label class="mb-0">entries</label>
    `,t.querySelector("select").addEventListener("change",s=>{this.options.perPage=parseInt(s.target.value),this.currentPage=1,this.render()}),this.topLeft.appendChild(t)}addSorting(){this.table.querySelectorAll("thead th").forEach((e,s)=>{e.style.cursor="pointer",e.style.userSelect="none",e.classList.add("sortable");const a=document.createElement("i");a.className="fas fa-sort ms-1 text-muted",a.style.fontSize="0.8em",e.appendChild(a),e.addEventListener("click",()=>{this.sort(s)})})}sort(t){this.sortColumn===t?this.sortDirection=this.sortDirection==="asc"?"desc":"asc":(this.sortColumn=t,this.sortDirection="asc"),this.table.querySelectorAll("thead th").forEach((s,a)=>{const r=s.querySelector("i");a===t?r.className=`fas fa-sort-${this.sortDirection==="asc"?"up":"down"} ms-1 text-primary`:r.className="fas fa-sort ms-1 text-muted"}),this.filteredRows.sort((s,a)=>{const r=s.cells[t].textContent.trim(),i=a.cells[t].textContent.trim(),n=parseFloat(r.replace(/[^0-9.-]/g,"")),l=parseFloat(i.replace(/[^0-9.-]/g,""));return!isNaN(n)&&!isNaN(l)?this.sortDirection==="asc"?n-l:l-n:this.sortDirection==="asc"?r.localeCompare(i):i.localeCompare(r)}),this.render()}filterRows(){this.searchTerm?this.filteredRows=this.originalRows.filter(t=>Array.from(t.cells).some(e=>e.textContent.toLowerCase().includes(this.searchTerm))):this.filteredRows=[...this.originalRows]}createPagination(){}render(){const t=this.table.querySelector("tbody");t.innerHTML="";const e=Math.ceil(this.filteredRows.length/this.options.perPage),s=(this.currentPage-1)*this.options.perPage,a=s+this.options.perPage;if(this.filteredRows.slice(s,a).forEach(i=>t.appendChild(i.cloneNode(!0))),this.bottomLeft.innerHTML=`
      Showing ${s+1} to ${Math.min(a,this.filteredRows.length)} 
      of ${this.filteredRows.length} entries
      ${this.searchTerm?` (filtered from ${this.originalRows.length} total)`:""}
    `,this.bottomRight.innerHTML="",e>1){const i=document.createElement("nav");i.innerHTML=`
        <ul class="pagination pagination-sm mb-0">
          <li class="page-item ${this.currentPage===1?"disabled":""}">
            <a class="page-link" href="#" data-page="prev">Previous</a>
          </li>
          ${this.generatePageNumbers(e)}
          <li class="page-item ${this.currentPage===e?"disabled":""}">
            <a class="page-link" href="#" data-page="next">Next</a>
          </li>
        </ul>
      `,i.addEventListener("click",n=>{n.preventDefault();const l=n.target.closest(".page-link");if(!l)return;const o=l.dataset.page;o==="prev"&&this.currentPage>1?this.currentPage--:o==="next"&&this.currentPage<e?this.currentPage++:o&&o!=="prev"&&o!=="next"&&(this.currentPage=parseInt(o)),this.render()}),this.bottomRight.appendChild(i)}}generatePageNumbers(t){const e=[];if(t<=5)for(let a=1;a<=t;a++)e.push(`
          <li class="page-item ${this.currentPage===a?"active":""}">
            <a class="page-link" href="#" data-page="${a}">${a}</a>
          </li>
        `);else{e.push(`
        <li class="page-item ${this.currentPage===1?"active":""}">
          <a class="page-link" href="#" data-page="1">1</a>
        </li>
      `),this.currentPage>3&&e.push('<li class="page-item disabled"><span class="page-link">...</span></li>');const a=Math.max(2,this.currentPage-1),r=Math.min(t-1,this.currentPage+1);for(let i=a;i<=r;i++)e.push(`
          <li class="page-item ${this.currentPage===i?"active":""}">
            <a class="page-link" href="#" data-page="${i}">${i}</a>
          </li>
        `);this.currentPage<t-2&&e.push('<li class="page-item disabled"><span class="page-link">...</span></li>'),e.push(`
        <li class="page-item ${this.currentPage===t?"active":""}">
          <a class="page-link" href="#" data-page="${t}">${t}</a>
        </li>
      `)}return e.join("")}}function g(c,t="export.csv"){const e=document.getElementById(c);if(!e)return;const s=e.querySelectorAll("tr"),a=[];s.forEach(l=>{const o=l.querySelectorAll("td, th"),p=Array.from(o).map(d=>`"${d.textContent.replace(/"/g,'""')}"`);a.push(p.join(","))});const r=new Blob([a.join(`
`)],{type:"text/csv"}),i=URL.createObjectURL(r),n=document.createElement("a");n.href=i,n.download=t,n.click(),URL.revokeObjectURL(i)}document.addEventListener("DOMContentLoaded",()=>{new h("dataTable",{perPage:10,perPageOptions:[5,10,25,50]});const c=document.getElementById("exportTable");if(c){new h("exportTable",{perPage:5,perPageOptions:[5,10,25]});const t=c.closest(".card").querySelector(".card-header");if(t){const e=document.createElement("button");e.className="btn btn-sm btn-primary float-end",e.innerHTML='<i class="fas fa-download me-1"></i> Export CSV',e.onclick=()=>g("exportTable","products.csv"),t.appendChild(e)}}});export{h as SimpleDataTable,g as exportTableToCSV};
