import { useState, useEffect } from 'react';
import {
    Card,
    Descriptions,
    Tag,
    Table,
    Button,
    Space,
    Typography,
    message,
    Spin,
    Empty,
    Row,
    Col,
} from 'antd';
import {
    ArrowLeftOutlined,
    FilePdfOutlined,
    EditOutlined,
    UserOutlined,
    HeartOutlined,
    SmileOutlined,
} from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import api from '../api/axios';
import dayjs from 'dayjs';

const { Title, Text } = Typography;

export default function EmployeeDetailPage() {
    const [employee, setEmployee] = useState(null);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();
    const { id } = useParams();
    const { isAdmin } = useAuth();

    useEffect(() => {
        fetchEmployee();
    }, [id]);

    const fetchEmployee = async () => {
        try {
            const res = await api.get(`/employees/${id}`);
            setEmployee(res.data);
        } catch (err) {
            message.error('Employee not found');
            navigate('/');
        } finally {
            setLoading(false);
        }
    };

    const handleExportCv = async () => {
        try {
            const res = await api.get(`/employees/${id}/export/cv`, {
                responseType: 'blob',
            });
            const url = window.URL.createObjectURL(new Blob([res.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `CV_${employee.name.replace(/\s+/g, '_')}.pdf`);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
            message.success('CV downloaded');
        } catch (err) {
            message.error('Failed to download CV');
        }
    };

    if (loading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', marginTop: 100 }}>
                <Spin size="large" />
            </div>
        );
    }

    if (!employee) {
        return <Empty description="Employee not found" />;
    }

    const departmentColors = {
        Engineering: 'blue',
        'Human Resources': 'green',
        Finance: 'gold',
        Administration: 'purple',
        Marketing: 'magenta',
    };

    const childColumns = [
        {
            title: '#',
            key: 'index',
            render: (_, __, i) => i + 1,
            width: 50,
        },
        {
            title: 'Name',
            dataIndex: 'name',
            key: 'name',
        },
        {
            title: 'Date of Birth',
            dataIndex: 'dateOfBirth',
            key: 'dateOfBirth',
            render: (dob) => dayjs(dob).format('DD MMM YYYY'),
        },
        {
            title: 'Age',
            key: 'age',
            render: (_, record) => {
                const age = dayjs().diff(dayjs(record.dateOfBirth), 'year');
                return `${age} years`;
            },
        },
    ];

    return (
        <div style={{ maxWidth: 900, margin: '0 auto' }}>
            <Space style={{ marginBottom: 16 }}>
                <Button type="text" icon={<ArrowLeftOutlined />} onClick={() => navigate('/')}>
                    Back to List
                </Button>
            </Space>

            {/* Employee Info */}
            <Card
                style={{ borderRadius: 12, marginBottom: 16 }}
                title={
                    <Space>
                        <UserOutlined style={{ color: '#1677ff', fontSize: 20 }} />
                        <Title level={4} style={{ margin: 0 }}>
                            {employee.name}
                        </Title>
                        <Tag color={departmentColors[employee.department] || 'default'}>
                            {employee.department}
                        </Tag>
                    </Space>
                }
                extra={
                    <Space>
                        <Button icon={<FilePdfOutlined />} onClick={handleExportCv} type="primary" ghost>
                            Download CV
                        </Button>
                        {isAdmin && (
                            <Button
                                icon={<EditOutlined />}
                                onClick={() => navigate(`/employees/${id}/edit`)}
                                type="primary"
                            >
                                Edit
                            </Button>
                        )}
                    </Space>
                }
            >
                <Descriptions bordered column={{ xs: 1, sm: 2 }}>
                    <Descriptions.Item label="Full Name">{employee.name}</Descriptions.Item>
                    <Descriptions.Item label="NID">
                        <code>{employee.nid}</code>
                    </Descriptions.Item>
                    <Descriptions.Item label="Phone">{employee.phone}</Descriptions.Item>
                    <Descriptions.Item label="Department">
                        <Tag color={departmentColors[employee.department]}>{employee.department}</Tag>
                    </Descriptions.Item>
                    <Descriptions.Item label="Basic Salary">
                        <Text strong style={{ color: '#52c41a', fontSize: 16 }}>
                            ৳{employee.basicSalary?.toLocaleString()}
                        </Text>
                        <Text type="secondary"> /month</Text>
                    </Descriptions.Item>
                </Descriptions>
            </Card>

            <Row gutter={16}>
                {/* Spouse */}
                <Col xs={24} md={12}>
                    <Card
                        style={{ borderRadius: 12, marginBottom: 16, height: '100%' }}
                        title={
                            <Space>
                                <HeartOutlined style={{ color: '#eb2f96' }} />
                                <span>Spouse Information</span>
                            </Space>
                        }
                    >
                        {employee.spouse ? (
                            <Descriptions column={1}>
                                <Descriptions.Item label="Name">{employee.spouse.name}</Descriptions.Item>
                                <Descriptions.Item label="NID">
                                    <code>{employee.spouse.nid}</code>
                                </Descriptions.Item>
                            </Descriptions>
                        ) : (
                            <Empty description="No spouse on record" image={Empty.PRESENTED_IMAGE_SIMPLE} />
                        )}
                    </Card>
                </Col>

                {/* Children */}
                <Col xs={24} md={12}>
                    <Card
                        style={{ borderRadius: 12, marginBottom: 16, height: '100%' }}
                        title={
                            <Space>
                                <SmileOutlined style={{ color: '#faad14' }} />
                                <span>Children ({employee.children?.length || 0})</span>
                            </Space>
                        }
                    >
                        {employee.children?.length > 0 ? (
                            <Table
                                columns={childColumns}
                                dataSource={employee.children}
                                rowKey="id"
                                pagination={false}
                                size="small"
                            />
                        ) : (
                            <Empty description="No children on record" image={Empty.PRESENTED_IMAGE_SIMPLE} />
                        )}
                    </Card>
                </Col>
            </Row>
        </div>
    );
}
