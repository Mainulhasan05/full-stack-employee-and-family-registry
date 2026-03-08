import { useState, useEffect, useCallback } from 'react';
import {
    Table,
    Input,
    Button,
    Space,
    Tag,
    Card,
    Typography,
    Popconfirm,
    message,
    Tooltip,
    Row,
    Col,
    Statistic,
} from 'antd';
import {
    SearchOutlined,
    PlusOutlined,
    EditOutlined,
    DeleteOutlined,
    FilePdfOutlined,
    EyeOutlined,
    TeamOutlined,
    DollarOutlined,
    BankOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import api from '../api/axios';

const { Title } = Typography;

export default function EmployeeListPage() {
    const [employees, setEmployees] = useState([]);
    const [loading, setLoading] = useState(false);
    const [search, setSearch] = useState('');
    const [searchDebounced, setSearchDebounced] = useState('');
    const { isAdmin } = useAuth();
    const navigate = useNavigate();

    // Debounce search — 300ms
    useEffect(() => {
        const timer = setTimeout(() => {
            setSearchDebounced(search);
        }, 300);
        return () => clearTimeout(timer);
    }, [search]);

    const fetchEmployees = useCallback(async () => {
        setLoading(true);
        try {
            const params = searchDebounced ? { search: searchDebounced } : {};
            const res = await api.get('/employees', { params });
            setEmployees(res.data);
        } catch (err) {
            message.error('Failed to fetch employees');
        } finally {
            setLoading(false);
        }
    }, [searchDebounced]);

    useEffect(() => {
        fetchEmployees();
    }, [fetchEmployees]);

    const handleDelete = async (id) => {
        try {
            await api.delete(`/employees/${id}`);
            message.success('Employee deleted');
            fetchEmployees();
        } catch (err) {
            message.error('Failed to delete employee');
        }
    };

    const handleExportTable = async () => {
        try {
            const params = searchDebounced ? { search: searchDebounced } : {};
            const res = await api.get('/employees/export/pdf', {
                params,
                responseType: 'blob',
            });
            const url = window.URL.createObjectURL(new Blob([res.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', 'Employee_List.pdf');
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
            message.success('PDF downloaded');
        } catch (err) {
            message.error('Failed to export PDF');
        }
    };

    const handleExportCv = async (id, name) => {
        try {
            const res = await api.get(`/employees/${id}/export/cv`, {
                responseType: 'blob',
            });
            const url = window.URL.createObjectURL(new Blob([res.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `CV_${name.replace(/\s+/g, '_')}.pdf`);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
        } catch (err) {
            message.error('Failed to export CV');
        }
    };

    const departmentColors = {
        Engineering: 'blue',
        'Human Resources': 'green',
        Finance: 'gold',
        Administration: 'purple',
        Marketing: 'magenta',
    };

    const columns = [
        {
            title: 'Name',
            dataIndex: 'name',
            key: 'name',
            render: (text, record) => (
                <Button type="link" onClick={() => navigate(`/employees/${record.id}`)} style={{ padding: 0 }}>
                    {text}
                </Button>
            ),
            sorter: (a, b) => a.name.localeCompare(b.name),
        },
        {
            title: 'NID',
            dataIndex: 'nid',
            key: 'nid',
            render: (text) => <code style={{ fontSize: 12 }}>{text}</code>,
        },
        {
            title: 'Phone',
            dataIndex: 'phone',
            key: 'phone',
        },
        {
            title: 'Department',
            dataIndex: 'department',
            key: 'department',
            render: (dept) => <Tag color={departmentColors[dept] || 'default'}>{dept}</Tag>,
            filters: [...new Set(employees.map((e) => e.department))].map((d) => ({
                text: d,
                value: d,
            })),
            onFilter: (value, record) => record.department === value,
        },
        {
            title: 'Salary (BDT)',
            dataIndex: 'basicSalary',
            key: 'basicSalary',
            render: (val) => `৳${val?.toLocaleString()}`,
            sorter: (a, b) => a.basicSalary - b.basicSalary,
            align: 'right',
        },
        {
            title: 'Family',
            key: 'family',
            render: (_, record) => {
                const parts = [];
                if (record.spouse) parts.push('Spouse');
                if (record.children?.length > 0) parts.push(`${record.children.length} child(ren)`);
                return parts.length > 0 ? (
                    <span style={{ fontSize: 12, color: '#666' }}>{parts.join(', ')}</span>
                ) : (
                    <span style={{ fontSize: 12, color: '#bbb' }}>—</span>
                );
            },
        },
        {
            title: 'Actions',
            key: 'actions',
            fixed: 'right',
            width: 180,
            render: (_, record) => (
                <Space>
                    <Tooltip title="View Details">
                        <Button
                            type="text"
                            icon={<EyeOutlined />}
                            onClick={() => navigate(`/employees/${record.id}`)}
                        />
                    </Tooltip>
                    <Tooltip title="Download CV">
                        <Button
                            type="text"
                            icon={<FilePdfOutlined />}
                            onClick={() => handleExportCv(record.id, record.name)}
                            style={{ color: '#cf1322' }}
                        />
                    </Tooltip>
                    {isAdmin && (
                        <>
                            <Tooltip title="Edit">
                                <Button
                                    type="text"
                                    icon={<EditOutlined />}
                                    onClick={() => navigate(`/employees/${record.id}/edit`)}
                                    style={{ color: '#1677ff' }}
                                />
                            </Tooltip>
                            <Popconfirm
                                title="Delete this employee?"
                                onConfirm={() => handleDelete(record.id)}
                                okText="Yes"
                                cancelText="No"
                            >
                                <Tooltip title="Delete">
                                    <Button type="text" icon={<DeleteOutlined />} danger />
                                </Tooltip>
                            </Popconfirm>
                        </>
                    )}
                </Space>
            ),
        },
    ];

    // Stats
    const totalSalary = employees.reduce((sum, e) => sum + (e.basicSalary || 0), 0);
    const departments = new Set(employees.map((e) => e.department));

    return (
        <div>
            {/* Stats Cards */}
            <Row gutter={16} style={{ marginBottom: 24 }}>
                <Col xs={24} sm={8}>
                    <Card style={{ borderRadius: 8 }}>
                        <Statistic
                            title="Total Employees"
                            value={employees.length}
                            prefix={<TeamOutlined />}
                            valueStyle={{ color: '#1677ff' }}
                        />
                    </Card>
                </Col>
                <Col xs={24} sm={8}>
                    <Card style={{ borderRadius: 8 }}>
                        <Statistic
                            title="Departments"
                            value={departments.size}
                            prefix={<BankOutlined />}
                            valueStyle={{ color: '#52c41a' }}
                        />
                    </Card>
                </Col>
                <Col xs={24} sm={8}>
                    <Card style={{ borderRadius: 8 }}>
                        <Statistic
                            title="Total Payroll"
                            value={totalSalary}
                            prefix={<DollarOutlined />}
                            suffix="BDT"
                            valueStyle={{ color: '#faad14' }}
                        />
                    </Card>
                </Col>
            </Row>

            <Card
                style={{ borderRadius: 8 }}
                title={
                    <Title level={4} style={{ margin: 0 }}>
                        Employee Directory
                    </Title>
                }
                extra={
                    <Space>
                        <Button icon={<FilePdfOutlined />} onClick={handleExportTable}>
                            Export PDF
                        </Button>
                        {isAdmin && (
                            <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/employees/new')}>
                                Add Employee
                            </Button>
                        )}
                    </Space>
                }
            >
                <Input
                    placeholder="Search by Name, NID, or Department..."
                    prefix={<SearchOutlined />}
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    allowClear
                    size="large"
                    style={{ marginBottom: 16, borderRadius: 8 }}
                    id="global-search-input"
                />
                <Table
                    columns={columns}
                    dataSource={employees}
                    rowKey="id"
                    loading={loading}
                    pagination={{
                        pageSize: 10,
                        showSizeChanger: true,
                        showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} employees`,
                    }}
                    scroll={{ x: 900 }}
                    style={{ borderRadius: 8 }}
                />
            </Card>
        </div>
    );
}
