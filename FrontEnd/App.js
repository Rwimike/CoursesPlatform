const API_URL = 'http://localhost:5054/api';

const { createApp } = Vue;

createApp({
    data() {
        return {
            isAuthenticated: false,
            userEmail: '',
            token: '',
            currentView: 'dashboard',
            loading: false,
            lessonsLoading: false,
            loggingIn: false,
            savingCourse: false,
            savingLesson: false,

            loginData: {
                email: 'test@courseplatform.com',
                password: 'Test123!'
            },
            loginError: '',

            courses: [],
            lessons: [],
            selectedCourse: null,

            filters: {
                search: '',
                status: ''
            },

            pagination: {
                currentPage: 1,
                pageSize: 9,
                totalPages: 0,
                totalItems: 0
            },

            metrics: {
                totalCourses: 0,
                publishedCourses: 0,
                draftCourses: 0,
                totalLessons: 0,
                recentCourses: []
            },

            modals: {
                course: false,
                lessons: false,
                lessonForm: false
            },

            courseForm: {
                title: ''
            },
            editingCourse: null,

            lessonForm: {
                title: '',
                order: 1,
                courseId: null
            },
            editingLesson: null
        };
    },

    mounted() {
        const token = localStorage.getItem('token');
        const email = localStorage.getItem('email');

        if (token && email) {
            this.token = token;
            this.userEmail = email;
            this.isAuthenticated = true;
            this.loadMetrics();
        }
    },

    methods: {
        // Navigation
        switchView(view) {
            this.currentView = view;
            if (view === 'dashboard') {
                this.loadMetrics();
            } else if (view === 'courses') {
                this.loadCourses();
            }
        },

        // Auth
        async login() {
            this.loggingIn = true;
            this.loginError = '';

            try {
                const response = await axios.post(`${API_URL}/Auth/login`, this.loginData);
                this.token = response.data.token;
                this.userEmail = response.data.email || this.loginData.email;
                this.isAuthenticated = true;

                localStorage.setItem('token', this.token);
                localStorage.setItem('email', this.userEmail);

                await this.loadMetrics();
            } catch (error) {
                this.loginError = error.response?.data?.message || 'Error al iniciar sesión. Verifica tus credenciales.';
                console.error('Login error:', error);
            } finally {
                this.loggingIn = false;
            }
        },

        logout() {
            this.isAuthenticated = false;
            this.token = '';
            this.userEmail = '';
            this.currentView = 'dashboard';
            localStorage.removeItem('token');
            localStorage.removeItem('email');
        },

        getAuthHeaders() {
            return {
                headers: {
                    'Authorization': `Bearer ${this.token}`,
                    'Content-Type': 'application/json'
                }
            };
        },

        // Metrics (calculado manualmente ya que no hay endpoint específico)
        async loadMetrics() {
            this.loading = true;
            try {
                // Cargar todos los cursos para calcular métricas
                const params = new URLSearchParams({
                    page: 1,
                    pageSize: 100
                });

                const response = await axios.get(
                    `${API_URL}/Courses/search?${params}`,
                    this.getAuthHeaders()
                );

                const allCourses = response.data.items || [];

                this.metrics.totalCourses = allCourses.length;
                this.metrics.publishedCourses = allCourses.filter(c => c.status === 1 || c.status === 'Published').length;
                this.metrics.draftCourses = allCourses.filter(c => c.status === 0 || c.status === 'Draft').length;

                // Ordenar por fecha y tomar los 5 más recientes
                this.metrics.recentCourses = allCourses
                    .sort((a, b) => new Date(b.updatedAt || b.createdAt) - new Date(a.updatedAt || a.createdAt))
                    .slice(0, 5);

                // Calcular total de lecciones
                let totalLessons = 0;
                for (const course of allCourses) {
                    try {
                        const lessonsResponse = await axios.get(
                            `${API_URL}/courses/${course.id}/Lessons`,
                            this.getAuthHeaders()
                        );
                        totalLessons += lessonsResponse.data.length || 0;
                    } catch (err) {
                        console.error(`Error loading lessons for course ${course.id}:`, err);
                    }
                }
                this.metrics.totalLessons = totalLessons;

            } catch (error) {
                console.error('Error loading metrics:', error);
                // Si hay error de autenticación, hacer logout
                if (error.response?.status === 401) {
                    this.logout();
                }
            } finally {
                this.loading = false;
            }
        },

        // Courses
        async loadCourses() {
            this.loading = true;
            try {
                const params = new URLSearchParams({
                    page: this.pagination.currentPage,
                    pageSize: this.pagination.pageSize
                });

                if (this.filters.search) {
                    params.append('q', this.filters.search);
                }
                if (this.filters.status !== '') {
                    params.append('status', this.filters.status);
                }

                const response = await axios.get(
                    `${API_URL}/Courses/search?${params}`,
                    this.getAuthHeaders()
                );

                if (response.data) {
                    this.courses = response.data.items || [];
                    this.pagination.totalPages = response.data.totalPages || 1;
                    this.pagination.totalItems = response.data.totalItems || 0;
                    this.pagination.currentPage = response.data.currentPage || 1;
                }
            } catch (error) {
                console.error('Error loading courses:', error);
                this.courses = [];
                if (error.response?.status === 401) {
                    this.logout();
                }
            } finally {
                this.loading = false;
            }
        },

        changePage(page) {
            if (page < 1 || page > this.pagination.totalPages) return;
            this.pagination.currentPage = page;
            this.loadCourses();
        },

        // Course CRUD
        openCreateCourseModal() {
            this.editingCourse = null;
            this.courseForm = { title: '' };
            this.modals.course = true;
        },

        openEditCourseModal(course) {
            this.editingCourse = course;
            this.courseForm = { title: course.title };
            this.modals.course = true;
        },

        closeCourseModal() {
            this.modals.course = false;
            this.editingCourse = null;
            this.courseForm = { title: '' };
        },

        async saveCourse() {
            this.savingCourse = true;
            try {
                if (this.editingCourse) {
                    await axios.put(
                        `${API_URL}/Courses/${this.editingCourse.id}`,
                        this.courseForm,
                        this.getAuthHeaders()
                    );
                } else {
                    await axios.post(
                        `${API_URL}/Courses`,
                        this.courseForm,
                        this.getAuthHeaders()
                    );
                }
                this.closeCourseModal();
                await this.loadCourses();
                await this.loadMetrics();
            } catch (error) {
                console.error('Error saving course:', error);
                alert('Error al guardar el curso: ' + (error.response?.data?.message || error.message));
            } finally {
                this.savingCourse = false;
            }
        },

        async publishCourse(courseId) {
            if (!confirm('¿Publicar este curso?')) return;

            try {
                const response = await axios.patch(
                    `${API_URL}/Courses/${courseId}/publish`,
                    {},
                    this.getAuthHeaders()
                );

                // Éxito
                alert('✅ ' + (response.data.message || 'Curso publicado exitosamente'));

                // Recargar datos
                await this.loadCourses();
                await this.loadMetrics();

            } catch (error) {
                console.error('Error detallado al publicar:', error);

                let errorMessage = 'Error al publicar el curso. ';

                if (error.response) {
                    switch (error.response.status) {
                        case 400:
                            // Error de validación (sin lecciones)
                            if (error.response.data?.code === 'NO_LESSONS') {
                                errorMessage = '❌ No se puede publicar un curso sin lecciones.\n\n' +
                                    'Por favor, agrega al menos una lección antes de publicar.';

                                // Preguntar si quiere ir a agregar lecciones
                                if (confirm(errorMessage + '\n\n¿Deseas agregar lecciones ahora?')) {
                                    // Buscar el curso para abrir el modal de lecciones
                                    const course = this.courses.find(c => c.id === courseId);
                                    if (course) {
                                        this.openLessonsModal(course);
                                    }
                                }
                                return; // Salir sin mostrar alerta adicional

                            } else {
                                errorMessage += 'Datos inválidos: ' +
                                    (error.response.data?.message || error.response.statusText);
                            }
                            break;

                        case 500:
                            // Error de ciclo JSON (backend mal configurado)
                            errorMessage = '⚠️  Error del servidor (configuración JSON).\n\n' +
                                'El backend tiene un problema de referencias circulares.\n' +
                                'Esto es un error de configuración del servidor.\n\n' +
                                'Pero el curso probablemente se publicó correctamente.\n' +
                                'Por favor, recarga la página para ver los cambios.';

                            // Forzar recarga de datos aunque haya error 500
                            setTimeout(async () => {
                                await this.loadCourses();
                                await this.loadMetrics();
                                alert('✅ Datos recargados. Verifica si el curso se publicó.');
                            }, 1000);
                            break;

                        default:
                            errorMessage += `Error ${error.response.status}: ${error.response.data?.message || error.response.statusText}`;
                            break;
                    }
                } else if (error.request) {
                    errorMessage += 'No se pudo conectar con el servidor.';
                } else {
                    errorMessage += error.message;
                }

                // Mostrar alerta solo si no es el caso de "sin lecciones"
                alert(errorMessage);
            }
        },

        async unpublishCourse(courseId) {
            if (!confirm('¿Cambiar a borrador?')) return;

            try {
                await axios.patch(
                    `${API_URL}/Courses/${courseId}/unpublish`,
                    {},
                    this.getAuthHeaders()
                );
                await this.loadCourses();
                await this.loadMetrics();
            } catch (error) {
                console.error('Error unpublishing course:', error);
                alert('Error al cambiar estado del curso: ' + (error.response?.data?.message || error.message));
            }
        },

        async deleteCourse(courseId) {
            if (!confirm('¿Estás seguro de eliminar este curso? Se eliminarán todas sus lecciones.')) return;

            try {
                await axios.delete(
                    `${API_URL}/Courses/${courseId}`,
                    this.getAuthHeaders()
                );
                await this.loadCourses();
                await this.loadMetrics();
            } catch (error) {
                console.error('Error deleting course:', error);
                alert('Error al eliminar el curso: ' + (error.response?.data?.message || error.message));
            }
        },

        // Lessons
        async openLessonsModal(course) {
            this.selectedCourse = course;
            this.modals.lessons = true;
            await this.loadLessons(course.id);
        },

        closeLessonsModal() {
            this.modals.lessons = false;
            this.selectedCourse = null;
            this.lessons = [];
        },

        async loadLessons(courseId) {
            this.lessonsLoading = true;
            try {
                const response = await axios.get(
                    `${API_URL}/courses/${courseId}/Lessons`,
                    this.getAuthHeaders()
                );
                this.lessons = response.data || [];
                // Ordenar por order
                this.lessons.sort((a, b) => a.order - b.order);
            } catch (error) {
                console.error('Error loading lessons:', error);
                this.lessons = [];
                if (error.response?.status === 401) {
                    this.logout();
                }
            } finally {
                this.lessonsLoading = false;
            }
        },

        openCreateLessonModal() {
            this.editingLesson = null;
            this.lessonForm = {
                title: '',
                order: this.lessons.length + 1,
                courseId: this.selectedCourse.id
            };
            this.modals.lessonForm = true;
        },

        openEditLessonModal(lesson) {
            this.editingLesson = lesson;
            this.lessonForm = {
                title: lesson.title,
                order: lesson.order,
                courseId: this.selectedCourse.id
            };
            this.modals.lessonForm = true;
        },

        closeLessonFormModal() {
            this.modals.lessonForm = false;
            this.editingLesson = null;
            this.lessonForm = { title: '', order: 1, courseId: null };
        },

        async saveLesson() {
            this.savingLesson = true;
            try {
                if (this.editingLesson) {
                    await axios.put(
                        `${API_URL}/courses/${this.selectedCourse.id}/Lessons/${this.editingLesson.id}`,
                        this.lessonForm,
                        this.getAuthHeaders()
                    );
                } else {
                    await axios.post(
                        `${API_URL}/courses/${this.selectedCourse.id}/Lessons`,
                        this.lessonForm,
                        this.getAuthHeaders()
                    );
                }
                this.closeLessonFormModal();
                await this.loadLessons(this.selectedCourse.id);
                await this.loadMetrics();
            } catch (error) {
                console.error('Error saving lesson:', error);
                alert('Error al guardar la lección: ' + (error.response?.data?.message || error.message));
            } finally {
                this.savingLesson = false;
            }
        },

        async deleteLesson(lessonId) {
            if (!confirm('¿Estás seguro de eliminar esta lección?')) return;

            try {
                await axios.delete(
                    `${API_URL}/courses/${this.selectedCourse.id}/Lessons/${lessonId}`,
                    this.getAuthHeaders()
                );
                await this.loadLessons(this.selectedCourse.id);
                await this.loadMetrics();
            } catch (error) {
                console.error('Error deleting lesson:', error);
                alert('Error al eliminar la lección: ' + (error.response?.data?.message || error.message));
            }
        },

        async reorderLesson(lessonId, moveUp) {
            try {
                await axios.patch(
                    `${API_URL}/courses/${this.selectedCourse.id}/Lessons/${lessonId}/reorder`,
                    { moveUp: moveUp },
                    this.getAuthHeaders()
                );
                await this.loadLessons(this.selectedCourse.id);
            } catch (error) {
                console.error('Error reordering lesson:', error);
                alert('Error al reordenar la lección: ' + (error.response?.data?.message || error.message));
            }
        },

        // Utils
        formatDate(dateString) {
            if (!dateString) return 'N/A';
            try {
                const date = new Date(dateString);
                return date.toLocaleDateString('es-ES', {
                    year: 'numeric',
                    month: 'short',
                    day: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
                });
            } catch (e) {
                return dateString;
            }
        }
    }
}).mount('#app');