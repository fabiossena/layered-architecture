// LayeredCrm.Web — client logic for the Layered CRM demo.
// Talks to the Presentation layer (REST API) only through HTTP.
//
// Routes (hash based):
//   #/contacts                    -> list
//   #/contacts/new                -> create form
//   #/contacts/12                 -> edit form (same screen, pre-filled)
//   #/opportunities[/new|/id]     -> same pattern

const MODULES = {
  contacts: {
    plural: "Contacts",
    singular: "Contact",
    basePath: "/api/contacts",
    columns: ["Name", "Email", "Status", "Company", ""]
  },
  opportunities: {
    plural: "Opportunities",
    singular: "Opportunity",
    basePath: "/api/opportunities",
    columns: ["Title", "Amount", "Contact", "Stage", ""]
  }
};

const STAGES = ["Qualification", "Proposal", "Negotiation", "Won", "Lost"];

const cache = { contacts: [], opportunities: [], companies: [] };
const route = { module: "contacts", mode: "list", id: null };

const $ = (id) => document.getElementById(id);

/* ------------------------- utilities ------------------------- */

function escapeHtml(value) {
  return String(value ?? "").replace(/[&<>"']/g, (c) => ({
    "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
  })[c]);
}

function showError(message) {
  const box = $("error");
  box.textContent = message;
  box.style.display = "block";
  $("success").style.display = "none";
}

function showSuccess(message) {
  const box = $("success");
  box.textContent = message;
  box.style.display = "block";
  $("error").style.display = "none";
}

function hideMessages() {
  $("error").style.display = "none";
  $("success").style.display = "none";
}

async function api(path, options = {}) {
  const response = await fetch(path, {
    headers: { "Content-Type": "application/json" },
    ...options
  });

  if (response.status === 204) return null;

  const body = await response.json().catch(() => null);

  if (!response.ok) {
    const details = body?.errors?.length ? "\n- " + body.errors.join("\n- ") : "";
    throw new Error(
      `${body?.error || "HTTP " + response.status}: ${body?.message || response.statusText}${details}`
    );
  }

  return body;
}

/* ------------------------- routing ------------------------- */

function parseRoute() {
  const parts = (location.hash.replace(/^#\/?/, "") || "contacts").split("/");
  const module = MODULES[parts[0]] ? parts[0] : "contacts";

  if (parts[1] === "new") return { module, mode: "form", id: null };
  if (parts[1] !== undefined && Number.isFinite(Number(parts[1]))) {
    return { module, mode: "form", id: Number(parts[1]) };
  }
  return { module, mode: "list", id: null };
}

let lastRouteKey = "";

async function render() {
  Object.assign(route, parseRoute());
  hideMessagesIfRouteChanged();

  // Menu highlight
  document.querySelectorAll("#menu a").forEach((a) =>
    a.classList.toggle("active", a.dataset.module === route.module)
  );

  try {
    await loadAll();
  } catch (error) {
    showError(error.message);
    return;
  }

  if (route.mode === "list") {
    renderList();
  } else {
    renderForm();
  }
}

// Only clears messages when the user actually navigates — otherwise the
// hashchange render fired right after a save would wipe the success message.
function hideMessagesIfRouteChanged() {
  const key = `${route.module}/${route.mode}/${route.id ?? ""}`;
  if (key !== lastRouteKey) {
    hideMessages();
    lastRouteKey = key;
  }
}

window.addEventListener("hashchange", render);

/* ------------------------- data ------------------------- */

async function loadAll() {
  const [contacts, opportunities, companies] = await Promise.all([
    api("/api/contacts"),
    api("/api/opportunities"),
    api("/api/companies")
  ]);
  cache.contacts = contacts;
  cache.opportunities = opportunities;
  cache.companies = companies;
}

/* ------------------------- list view ------------------------- */

function renderList() {
  const module = MODULES[route.module];
  $("list-view").hidden = false;
  $("form-view").hidden = true;
  $("list-title").textContent = module.plural;
  $("btn-new").textContent = `+ New ${module.singular.toLowerCase()}`;
  $("list-head").innerHTML =
    `<tr>${module.columns.map((c) => `<th>${c}</th>`).join("")}</tr>`;

  const records = cache[route.module];
  $("list-count").textContent =
    records.length === 1 ? "1 record" : `${records.length} records`;

  const body = $("list-body");
  if (!records.length) {
    body.innerHTML =
      `<tr><td colspan="${module.columns.length}" class="empty">No records yet — click "+ New" to create one.</td></tr>`;
    return;
  }

  body.innerHTML = records
    .map((r) =>
      route.module === "contacts" ? contactRow(r) : opportunityRow(r)
    )
    .join("");
}

function contactRow(c) {
  return `
    <tr>
      <td><strong>${escapeHtml(c.fullName)}</strong></td>
      <td>${escapeHtml(c.email)}</td>
      <td><span class="badge">${escapeHtml(c.status)}</span></td>
      <td>${escapeHtml(c.companyName ?? "—")}</td>
      <td class="actions">
        <button class="small" data-action="edit" data-id="${c.id}">Edit</button>
        <button class="small danger" data-action="delete" data-id="${c.id}">Delete</button>
      </td>
    </tr>`;
}

function opportunityRow(o) {
  return `
    <tr>
      <td><strong>${escapeHtml(o.title)}</strong></td>
      <td class="amount">$${Number(o.amount).toLocaleString()}</td>
      <td>${escapeHtml(o.contactName)}</td>
      <td><span class="badge ${o.stage.toLowerCase()}">${escapeHtml(o.stage)}</span></td>
      <td class="actions">
        <button class="small" data-action="edit" data-id="${o.id}">Edit</button>
        <button class="small danger" data-action="delete" data-id="${o.id}">Delete</button>
      </td>
    </tr>`;
}

/* ------------------------- form view (new + edit) ------------------------- */

function renderForm() {
  $("list-view").hidden = true;
  $("form-view").hidden = false;

  const isEdit = route.id !== null;
  const module = MODULES[route.module];

  $("contact-fields").hidden = route.module !== "contacts";
  $("opportunity-fields").hidden = route.module !== "opportunities";
  $("o-stage-group").hidden = !(route.module === "opportunities" && isEdit);

  $("form-title").textContent =
    `${isEdit ? "Edit" : "New"} ${module.singular.toLowerCase()}` +
    (isEdit ? ` #${route.id}` : "");
  $("btn-save").textContent = isEdit ? "Save changes" : "Create";

  if (route.module === "contacts") fillContactForm();
  else fillOpportunityForm();
}

function fillContactForm() {
  const record = route.id !== null
    ? cache.contacts.find((c) => c.id === route.id)
    : null;

  if (route.id !== null && !record) {
    showError(`Contact #${route.id} was not found.`);
  }

  $("c-firstname").value = record?.firstName ?? "";
  $("c-lastname").value = record?.lastName ?? "";
  $("c-email").value = record?.email ?? "";
  $("c-phone").value = record?.phone ?? "";
  $("c-status").value = record?.status ?? "New";
  $("c-company").innerHTML =
    '<option value="">No company</option>' +
    cache.companies
      .map((c) => `<option value="${c.id}">${escapeHtml(c.name)}</option>`)
      .join("");
  $("c-company").value = record?.companyId ? String(record.companyId) : "";
}

function fillOpportunityForm() {
  const record = route.id !== null
    ? cache.opportunities.find((o) => o.id === route.id)
    : null;

  if (route.id !== null && !record) {
    showError(`Opportunity #${route.id} was not found.`);
  }

  $("o-title").value = record?.title ?? "";
  $("o-amount").value = record?.amount ?? "";
  $("o-close").value = record?.expectedCloseDate
    ? record.expectedCloseDate.slice(0, 10)
    : "";
  $("o-contact").innerHTML =
    '<option value="">Select a contact</option>' +
    cache.contacts
      .map((c) => `<option value="${c.id}">${escapeHtml(c.fullName)}</option>`)
      .join("");
  $("o-contact").value = record ? String(record.contactId) : "";
  $("o-stage").value = record?.stage ?? "Qualification";
}

/* ------------------------- save ------------------------- */

$("record-form").addEventListener("submit", async (event) => {
  event.preventDefault();
  hideMessages();

  const isEdit = route.id !== null;
  const module = MODULES[route.module];
  const base = module.basePath;
  const path = isEdit ? `${base}/${route.id}` : base;
  const method = isEdit ? "PUT" : "POST";

  let payload;
  if (route.module === "contacts") {
    payload = {
      firstName: $("c-firstname").value,
      lastName: $("c-lastname").value,
      email: $("c-email").value,
      phone: $("c-phone").value || null,
      status: $("c-status").value,
      companyId: $("c-company").value ? Number($("c-company").value) : null
    };
  } else {
    payload = {
      title: $("o-title").value,
      amount: Number($("o-amount").value),
      contactId: Number($("o-contact").value),
      expectedCloseDate: $("o-close").value
        ? new Date($("o-close").value + "T00:00:00Z").toISOString()
        : null
    };
    if (isEdit) payload.stage = $("o-stage").value;
  }

  try {
    // Opportunity stage changes go through the dedicated endpoint, which
    // enforces the transition rules (invalid moves -> 409).
    if (isEdit && route.module === "opportunities") {
      const original = cache.opportunities.find((o) => o.id === route.id);
      if (original && payload.stage !== original.stage) {
        await api(`${base}/${route.id}/stage`, {
          method: "PUT",
          body: JSON.stringify({ stage: payload.stage })
        });
      }
      delete payload.stage; // PUT /{id} does not change the stage
    }

    await api(path, { method, body: JSON.stringify(payload) });
    location.hash = `#/${route.module}`;
    await render();
    showSuccess(
      `${module.singular} ${isEdit ? "updated" : "created"}.`
    );
  } catch (error) {
    showError(error.message);
  }
});

/* ------------------------- row actions: edit + delete ------------------------- */

document.addEventListener("click", async (event) => {
  const button = event.target.closest("button[data-action]");
  if (!button) return;

  const id = Number(button.dataset.id);
  const action = button.dataset.action;
  const module = MODULES[route.module];

  if (action === "edit") {
    location.hash = `#/${route.module}/${id}`;
    return;
  }

  if (action === "delete") {
    const record = cache[route.module].find((r) => r.id === id);
    const label =
      route.module === "contacts"
        ? `contact "${record?.fullName ?? id}"`
        : `opportunity "${record?.title ?? id}"`;

    const extra =
      route.module === "contacts"
        ? "\n\nIts opportunities will also be deleted (cascade)."
        : "";

    if (!confirm(`Delete the ${label}?${extra}`)) return;

    hideMessages();
    try {
      await api(`${module.basePath}/${id}`, { method: "DELETE" });
      await render();
      showSuccess(`${module.singular} deleted.`);
    } catch (error) {
      showError(error.message);
      await render();
    }
  }
});

/* ------------------------- toolbar buttons ------------------------- */

$("btn-new").addEventListener("click", () => {
  location.hash = `#/${route.module}/new`;
});

function backToList() {
  location.hash = `#/${route.module}`;
}

$("btn-back").addEventListener("click", backToList);
$("btn-cancel").addEventListener("click", backToList);

/* ------------------------- start ------------------------- */

if (!location.hash) location.hash = "#/contacts";
render();
