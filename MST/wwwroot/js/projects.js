//projects.js
const table = document.getElementById("projectTable");
const form = document.getElementById("projectForm");
const nameInput = document.getElementById("projectName");
const locationInput = document.getElementById("projectLocation");
const descriptionInput = document.getElementById("projectDescription");
const statusInput = document.getElementById("projectStatus");
const imageInput = document.getElementById("projectImage");
const imageListInput = document.getElementById("projectImageList");
const addBtn = document.getElementById("addBtn");
const updateBtn = document.getElementById("updateBtn");
const idInput = document.getElementById("projectId");
const clearBtn = document.getElementById("clearBtn");
const startDateInput = document.getElementById("projectStartDate");
const endDateInput = document.getElementById("projectEndDate");
const imagePreview = document.getElementById("imagePreview");
const imageListPreview = document.getElementById("imageListPreview");

function getAntiforgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
}

async function loadProjects() {
    const response = await fetch('/Project/GetProjects');
    const projects = await response.json();
    table.innerHTML = '';
    projects.forEach((project, index) => {
        // Parse ImageList JSON
        let imageList = [];
        try {
            imageList = project.imageList ? JSON.parse(project.imageList) : [];
        } catch {
            console.error('Failed to parse ImageList for project', project.id);
        }

        const tr = document.createElement('tr');
        tr.innerHTML = `
                <td>${index + 1}</td>
                <td>${escapeHtml(project.name)}</td>
                <td>${escapeHtml(project.location)}</td>
                <td>${project.description ? escapeHtml(project.description) : 'No description'}</td>
                <td>${escapeHtml(project.status)}</td>
                <td>${project.startDate ? new Date(project.startDate).toLocaleDateString() : ''}</td>
                <td>${project.endDate ? new Date(project.endDate).toLocaleDateString() : ''}</td>
                <td>${project.thumbnail ? `<img src="/Uploads/${project.thumbnail}" alt="Project Image" width="100" />` : 'No image'}</td>
                <td>${imageList.length > 0
                ? imageList.map(img => `<img src="/Uploads/${img}" alt="Additional Image" width="100" class="me-2" />`).join('')
                : 'No additional images'
            }</td>
                <td>
                    <button class="btn btn-sm btn-warning editBtn" data-id="${project.id}">Edit</button>
                    <button class="btn btn-sm btn-danger deleteBtn" data-id="${project.id}">Delete</button>
                </td>
            `;
        table.appendChild(tr);
    });
}

function escapeHtml(unsafe) {
    if (unsafe === null || unsafe === undefined) return '';
    return String(unsafe)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function clearForm() {
    form.reset();
    idInput.removeAttribute('name');
    idInput.value = '';
    updateBtn.disabled = true;
    addBtn.disabled = false;
    imagePreview.innerHTML = '';
    imageListPreview.innerHTML = '';
    startDateInput.value = '';
    endDateInput.value = '';
}

clearBtn.addEventListener('click', () => {
    setTimeout(() => clearForm(), 0);
});

addBtn.addEventListener("click", async () => {
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    idInput.removeAttribute('name');
    const formData = new FormData(form);

    const response = await fetch('/Project/AddProject', {
        method: 'POST',
        headers: {
            'RequestVerificationToken': getAntiforgeryToken()
        },
        body: formData
    });

    if (response.ok) {
        clearForm();
        await loadProjects();
        alert("Project added successfully!");
    } else {
        let errorText = "Error adding project!";
        try {
            const errorData = await response.json();
            if (errorData.errors) errorText = errorData.errors.join(", ");
            else if (errorData.error) errorText = errorData.error;
        } catch { }
        alert(errorText);
    }
});

updateBtn.addEventListener("click", async () => {
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    idInput.setAttribute('name', 'Id');
    const formData = new FormData(form);

    const response = await fetch('/Project/UpdateProject', {
        method: 'PUT',
        headers: {
            'RequestVerificationToken': getAntiforgeryToken()
        },
        body: formData
    });

    if (response.ok) {
        clearForm();
        await loadProjects();
        alert("Project updated successfully!");
    } else {
        let errorText = "Error updating project!";
        try {
            const err = await response.json();
            if (err.errors) errorText = err.errors.join(", ");
            else if (err.error) errorText = err.error;
        } catch { }
        alert(errorText);
    }
});

table.addEventListener('click', async (e) => {
    const btn = e.target;
    if (btn.classList.contains('editBtn')) {
        const id = btn.getAttribute('data-id');
        if (!id) return;
        const resp = await fetch(`/Project/GetProject?id=${id}`);
        if (!resp.ok) {
            alert('Failed to fetch project data');
            return;
        }
        const project = await resp.json();
        startDateInput.value = project.startDate ? project.startDate.split('T')[0] : '';
        endDateInput.value = project.endDate ? project.endDate.split('T')[0] : '';
        nameInput.value = project.name || '';
        locationInput.value = project.location || '';
        descriptionInput.value = project.description || '';
        statusInput.value = project.status || 'Planned';
        idInput.value = project.id;
        idInput.setAttribute('name', 'Id');
        addBtn.disabled = true;
        updateBtn.disabled = false;

        // Display Thumbnail
        imagePreview.innerHTML = project.thumbnail
            ? `<img src="/Uploads/${project.thumbnail}" alt="Current Image" width="150" class="img-thumbnail mt-2" />`
            : `<p class="text-muted">No thumbnail uploaded</p>`;

        // Display ImageList
        let imageList = [];
        try {
            imageList = project.imageList ? JSON.parse(project.imageList) : [];
        } catch {
            console.error('Failed to parse ImageList');
        }
        imageListPreview.innerHTML = imageList.length > 0
            ? imageList.map(img => `<img src="/Uploads/${img}" alt="Additional Image" width="150" class="img-thumbnail mt-2 me-2" />`).join('')
            : `<p class="text-muted">No additional images uploaded</p>`;

        window.scrollTo({ top: 0, behavior: 'smooth' });
    } else if (btn.classList.contains('deleteBtn')) {
        const id = btn.getAttribute('data-id');
        if (!id) return;
        if (!confirm('Delete this project?')) return;
        const resp = await fetch(`/Project/DeleteProject?id=${id}`, {
            method: 'DELETE'
        });
        if (resp.ok) {
            await loadProjects();
            alert('Project deleted');
        } else {
            alert('Failed to delete project');
        }
    }
});

// Preview for Thumbnail
imageInput.addEventListener('change', () => {
    imagePreview.innerHTML = '';
    if (imageInput.files && imageInput.files[0]) {
        const reader = new FileReader();
        reader.onload = (e) => {
            imagePreview.innerHTML = `<img src="${e.target.result}" alt="Thumbnail Preview" width="150" class="img-thumbnail mt-2" />`;
        };
        reader.readAsDataURL(imageInput.files[0]);
    }
});

// Preview for ImageList
imageListInput.addEventListener('change', () => {
    imageListPreview.innerHTML = '';
    if (imageListInput.files && imageListInput.files.length > 0) {
        Array.from(imageListInput.files).forEach(file => {
            const reader = new FileReader();
            reader.onload = (e) => {
                imageListPreview.innerHTML += `<img src="${e.target.result}" alt="Additional Image Preview" width="150" class="img-thumbnail mt-2 me-2" />`;
            };
            reader.readAsDataURL(file);
        });
    }
});

loadProjects();